using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;
using CatalogService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();

        // Use in-memory DB for demo/testing
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseInMemoryDatabase("CatalogDb"));

        return services;
    }
}