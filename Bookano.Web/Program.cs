using Bookano.Web.Core.Mapping;
using Bookano.Web.Helpers;
using Bookano.Web.Seeds;
using Bookano.Web.Services.Image;
using Bookano.Web.Services.Mail;
using Bookano.Web.Tasks;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
    options.ValidationInterval = TimeSpan.FromMinutes(5)
);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder
    .Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddDataProtection().SetApplicationName(nameof(Bookano));

builder.Services.AddScoped<
    IUserClaimsPrincipalFactory<ApplicationUser>,
    ApplicationUserClaimsPrincipalFactory
>();

builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
builder.Services.AddHangfireServer();

builder.Services.Configure<AuthorizationOptions>(options =>
{
    options.AddPolicy(
        "AdminsOnly",
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(AppRoles.Admin);
        }
    );
});

builder.Services.AddScoped<SubscriptionJobs>();
builder.Services.AddKeyedTransient<IImageService, CloudinaryImageService>("cloudinary");
builder.Services.AddKeyedTransient<IImageService, LocalImageService>("local");
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
builder.Services.AddExpressiveAnnotations();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection(nameof(CloudinarySettings))
);
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

builder.Services.AddWhatsAppApiClient(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    await DefaultRoles.SeedAsync(
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>()
    );

    await DefaultUsers.SeedAdminUserAsync(
        scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()
    );
}

app.MapStaticAssets();

app.UseHangfireDashboard(
    "/hangfire",
    new DashboardOptions()
    {
        DashboardTitle = "Bookano Dashboard",
        IsReadOnlyFunc = (context => true),
        Authorization = [new HangfireAuthorizationFilter("AdminsOnly")],
    }
);

RecurringJob.AddOrUpdate<SubscriptionJobs>(
    "prepare-expiration-alerts",
    jobs => jobs.PrepareExpirationAlerts(),
    "0 12 * * *"
);

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
