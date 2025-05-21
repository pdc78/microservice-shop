using BasketService.Application.Interfaces;
using BasketService.Application.Services;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<BasketDbContext>(options =>
            options.UseInMemoryDatabase("BasketDb"));

        builder.Services.AddScoped<IBasketRepository, BasketRepository>();

        // Register repositories and services
        builder.Services.AddScoped<ICartService, CartService>();
        builder.Services.AddControllers();
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        //app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers();
        app.Run();
    }
}