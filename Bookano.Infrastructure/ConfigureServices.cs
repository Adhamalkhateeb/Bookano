using Bookano.Infrastructure.Persistence;
using Bookano.Infrastructure.Persistence.Interceptors;
using Bookano.Infrastructure.Services;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace Bookano.Infrastructure
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found."
                );

            services.AddDbContext<ApplicationDbContext>(
                (sp, options) =>
                {
                    options
                        .UseSqlServer(
                            connectionString,
                            builder =>
                                builder.MigrationsAssembly(
                                    typeof(ApplicationDbContext).Assembly.FullName
                                )
                        )
                        .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
                }
            );

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddHttpContextAccessor();
            services.AddScoped<AuditInterceptor>();

            // Hangfire
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();

            // Serilog → MSSqlServer
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.MSSqlServer(
                    connectionString,
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
                            new SqlColumn(
                                "UserId",
                                System.Data.SqlDbType.NVarChar,
                                dataLength: 450
                            ),
                            new SqlColumn(
                                "UserName",
                                System.Data.SqlDbType.NVarChar,
                                dataLength: 256
                            ),
                        ],
                    }
                )
                .CreateLogger();

            return services;
        }
    }
}
