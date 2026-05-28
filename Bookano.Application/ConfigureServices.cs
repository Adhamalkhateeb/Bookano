using System.Reflection;
using Bookano.Application.Services.Areas;
using Bookano.Application.Services.Authors;
using Bookano.Application.Services.Categories;
using Bookano.Application.Services.Governorates;
using Bookano.Application.Services.Publishers;
using Microsoft.Extensions.DependencyInjection;

namespace Bookano.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(_ => { }, Assembly.GetExecutingAssembly());

            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IPublisherService, PublisherService>();
            services.AddScoped<IGovernorateService, GovernorateService>();
            return services;
        }
    }
}
