using CatalogService.Application.Interfaces;
using CatalogService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductSetrvice, ProductService>();
        return services;
    }
}