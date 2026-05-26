using Bookano.Infrastructure.BackgroundServices;
using Bookano.Infrastructure.Persistence;
using Bookano.Infrastructure.Persistence.Interceptors;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.Configure<SecurityStampValidatorOptions>(options =>
                options.ValidationInterval = TimeSpan.FromMinutes(5)
            );

            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddHttpContextAccessor();
            services.AddScoped<AuditInterceptor>();

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

            services.AddScoped<IWhatsAppService<Subscriber>, WhatsAppService>();

            services.AddScoped<SubscriptionJobs>();
            services.AddScoped<RentalJobs>();

            // Hangfire
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();

            return services;
        }
    }
}
