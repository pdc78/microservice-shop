using BasketService.Application.Interfaces;
using BasketService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BasketService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICartService, CartService>();
        return services;
    }
}
