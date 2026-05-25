using System;
using Microsoft.Extensions.DependencyInjection;

namespace Bookano.Application
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            return services;
        }
    }
}
