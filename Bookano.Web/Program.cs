using Bookano.Web.Core.Mapping;
using Bookano.Web.Helpers;
using Bookano.Web.Seeds;
using Bookano.Web.Services.Image;
using Bookano.Web.Services.Mail;
using Bookano.Web.Services.PDF;
using Bookano.Web.Tasks;
using Hangfire;
using HashidsNet;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Context;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();

builder.Host.UseSerilog();

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
builder.Services.AddScoped<RentalJobs>();
builder.Services.AddKeyedTransient<IImageService, CloudinaryImageService>("cloudinary");
builder.Services.AddKeyedTransient<IImageService, LocalImageService>("local");
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
builder.Services.AddScoped<IViewRendererService, ViewRendererService>();

builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
builder.Services.AddExpressiveAnnotations();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection(nameof(CloudinarySettings))
);
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

builder.Services.AddWhatsAppApiClient(builder.Configuration);
builder.Services.AddScoped<WhatsAppHelper>();

builder.Services.AddSingleton<IHashids>(_ => new Hashids(salt: "f1nd1ngn3m0", minHashLength: 11));

builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseHsts();
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy(new CookiePolicyOptions { Secure = CookieSecurePolicy.Always });
app.Use(
    async (context, next) =>
    {
        context.Response.Headers.Append("X-Frame-Options", "Deny");
        await next();
    }
);

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(
    async (context, next) =>
    {
        using (
            LogContext.PushProperty(
                "UserId",
                context.User.FindFirstValue(ClaimTypes.NameIdentifier)
            )
        )
        using (LogContext.PushProperty("UserName", context.User.FindFirstValue(ClaimTypes.Name)))
        {
            await next();
        }
    }
);

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
        //IsReadOnlyFunc = (context => true),
        Authorization = [new HangfireAuthorizationFilter("AdminsOnly")],
    }
);

RecurringJob.AddOrUpdate<SubscriptionJobs>(
    "prepare-expiration-alerts",
    jobs => jobs.PrepareExpirationAlerts(),
    "0 12 * * *"
);

RecurringJob.AddOrUpdate<RentalJobs>(
    "prepare-rental-alerts",
    jobs => jobs.SendExpiringSoonAlerts(),
    "0 12 * * *"
);

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
