using Bookano.Infrastructure;
using Bookano.Infrastructure.BackgroundServices;
using Bookano.Infrastructure.Persistence.Seeds;
using Bookano.Web;
using Hangfire;
using HashidsNet;
using Serilog;
using Serilog.Context;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(
    (ctx, config) =>
    {
        config
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.MSSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs",
                    SchemaName = "logging",
                    AutoCreateSqlTable = true,
                },
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                columnOptions: new ColumnOptions
                {
                    AdditionalColumns =
                    [
                        new SqlColumn("UserId", System.Data.SqlDbType.NVarChar, dataLength: 450),
                        new SqlColumn("UserName", System.Data.SqlDbType.NVarChar, dataLength: 256),
                    ],
                }
            );
    }
);

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

await DatabaseInitializer.SeedAsync(app.Services);

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
