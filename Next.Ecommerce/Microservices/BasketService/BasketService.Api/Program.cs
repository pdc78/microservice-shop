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
            //http://localhost:5075/openapi/v1.json
            app.MapOpenApi();

            //http://localhost:5075/swagger/
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "OpenApi V1");
            });

            ////http://localhost:5075/api-docs
            //app.UseReDoc(options =>
            //{
            //    options.SpecUrl("/openapi/v1.json");
            //});

            ////http://localhost:5075/scalar
            //app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        //app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers();
        app.Run();
    }
}