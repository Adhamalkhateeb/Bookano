using Bookano.Application.Interfaces;
using Bookano.Infrastructure.BackgroundServices;
using Bookano.Infrastructure.Identity;
using Bookano.Infrastructure.Persistence;
using Bookano.Infrastructure.Persistence.Interceptors;
using Bookano.Infrastructure.Persistence.Repositories;
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
                        .AddInterceptors(sp.GetRequiredService<AuditableInterceptor>())
                        .LogTo(
                            Console.WriteLine,
                            Microsoft.Extensions.Logging.LogLevel.Information
                        );
                }
            );

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.Configure<SecurityStampValidatorOptions>(options =>
                options.ValidationInterval = TimeSpan.FromMinutes(5)
            );

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
            services.AddScoped<IWhatsAppService, WhatsAppService>();

            services.AddHttpContextAccessor();
            services.AddScoped<AuditableInterceptor>();


            services.AddScoped<SubscriptionJobs>();
            services.AddScoped<RentalJobs>();

            // Hangfire
            services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            services.AddHangfireServer();

            return services;
        }
    }
}
