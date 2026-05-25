using Bookano.Infrastructure;
using Bookano.Web;
using Bookano.Web.Seeds;
using Bookano.Web.Tasks;
using Hangfire;
using HashidsNet;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebServices(builder);

var app = builder.Build();

app.Use(
    async (context, next) =>
    {
        context.Response.Headers.Append("X-Frame-Options", "Deny");
        await next();
    }
);

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

app.UseSerilogRequestLogging();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(
    async (context, next) =>
    {
        using (LogContext.PushProperty("UserId", context.User.GetUserId()))
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
        IsReadOnlyFunc = (context => true),
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
