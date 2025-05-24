using BasketService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;

namespace BasketService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<BasketDbContext>(options =>
            options.UseInMemoryDatabase("BasketDb"));

        services.AddScoped<IBasketRepository, BasketRepository>();

        return services;
    }
}
