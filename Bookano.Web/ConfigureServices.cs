using Bookano.Infrastructure.Persistence;
using Bookano.Infrastructure.Settings;
using Bookano.Web.Core.Mapping;
using Bookano.Web.Helpers;
using Bookano.Web.Services.Image;
using Bookano.Web.Services.PDF;
using Bookano.Web.Validators;
using FluentValidation.AspNetCore;
using HashidsNet;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
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
            services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Password.RequiredLength = 8;
                    options.User.RequireUniqueEmail = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

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



            services.AddScoped<IViewRendererService, ViewRendererService>();

            services.AddControllersWithViews();
            services.AddMvc(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
            services.AddExpressiveAnnotations();

            services.AddWhatsAppApiClient(builder.Configuration);

            services.Configure<WhatsAppSettings>(config =>
            {
                builder.Configuration.GetSection("WhatsAppSettings").Bind(config);
                config.IsDevelopment = builder.Environment.IsDevelopment();
                config.DevelopmentOverrideMobile = "01021094971";
            });

            services.Configure<CloudinarySettings>(
                builder.Configuration.GetSection(nameof(CloudinarySettings))
            );

            services.Configure<MailSettings>(config =>
            {
                builder.Configuration.GetSection("MailSettings").Bind(config);

                config.IsDevelopment = builder.Environment.IsDevelopment();

                if (!Path.IsPathRooted(config.TemplatesPath))
                    config.TemplatesPath = Path.Combine(
                        builder.Environment.WebRootPath,
                        config.TemplatesPath
                    );
            });

            services.AddSingleton<IHashids>(_ => new Hashids(
                salt: "f1nd1ngn3m0",
                minHashLength: 11
            ));

            services.AddFluentValidationAutoValidation();
            services.AddFluentValidationClientsideAdapters();
            services.AddValidatorsFromAssemblyContaining<AreaFormViewModelValidator>();

            return services;
        }
    }
}
