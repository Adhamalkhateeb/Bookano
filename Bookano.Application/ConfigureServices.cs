using System.Reflection;
using Bookano.Application.Services.Areas;
using Bookano.Application.Services.Authors;
using Bookano.Application.Services.BookCopies;
using Bookano.Application.Services.Books;
using Bookano.Application.Services.Categories;
using Bookano.Application.Services.Publishers;
using Bookano.Application.Services.Rentals;
using Bookano.Application.Services.Subscribers;
using Bookano.Application.Services.Users;
using Bookano.Application.Services.Reports;
using Microsoft.Extensions.DependencyInjection;
using Bookano.Application.Services.Governorates;

namespace Bookano.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(_ => { }, Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(PaginationQueryBuilder<>));

            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IBookCopiesService, BookCopiesService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IRentalValidationService, RentalValidationService>();
            services.AddScoped<IRentalService, RentalService>();
            services.AddScoped<IPublisherService, PublisherService>();
            services.AddScoped<IGovernorateService, GovernorateService>();
            services.AddScoped<ISubscriberService, SubscriberService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IReportsService, ReportsService>();
            services.AddScoped<Bookano.Application.Services.Subscriptions.ISubscriptionService, Bookano.Application.Services.Subscriptions.SubscriptionService>();
            return services;
        }
    }
}
