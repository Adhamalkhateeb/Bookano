using Bookano.Infrastructure.Persistence;
using Bookano.Web.Core.Mapping;
using Bookano.Web.Helpers;
using Bookano.Web.Services.Image;
using Bookano.Web.Services.Mail;
using Bookano.Web.Services.PDF;
using Bookano.Web.Tasks;
using HashidsNet;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Serilog;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using WhatsAppCloudApi.Extensions;

namespace Bookano.Web
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddWebServices(
            this IServiceCollection services,
            WebApplicationBuilder builder
        )
        {
            builder.Host.UseSerilog();

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

            services.Configure<SecurityStampValidatorOptions>(options =>
                options.ValidationInterval = TimeSpan.FromMinutes(5)
            );

            services.AddScoped<
                IUserClaimsPrincipalFactory<ApplicationUser>,
                ApplicationUserClaimsPrincipalFactory
            >();

            services.AddDataProtection().SetApplicationName(nameof(Bookano));

            services.Configure<AuthorizationOptions>(options =>
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

            services.AddScoped<SubscriptionJobs>();
            services.AddScoped<RentalJobs>();

            services.AddKeyedTransient<IImageService, CloudinaryImageService>("cloudinary");
            services.AddKeyedTransient<IImageService, LocalImageService>("local");

            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();
            services.AddScoped<IViewRendererService, ViewRendererService>();

            services.AddControllersWithViews();
            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
            services.AddExpressiveAnnotations();

            services.AddWhatsAppApiClient(builder.Configuration);
            services.AddScoped<WhatsAppHelper>();

            services.Configure<CloudinarySettings>(
                builder.Configuration.GetSection(nameof(CloudinarySettings))
            );
            services.Configure<MailSettings>(
                builder.Configuration.GetSection(nameof(MailSettings))
            );

            services.AddSingleton<IHashids>(_ => new Hashids(
                salt: "f1nd1ngn3m0",
                minHashLength: 11
            ));

            return services;
        }
    }
}
