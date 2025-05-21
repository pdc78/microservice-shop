using CatalogService.Api.Middlewares;
using CatalogService.API.Data;
using CatalogService.Application.Interfaces;
using CatalogService.Application.Services;
using CatalogService.Infrastructure.Data;
using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register repositories and services
builder.Services.AddScoped<IProductSetrvice, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();


builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseInMemoryDatabase("CatalogDb"));  // For testing/demo purposes


builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    DbInitializer.Seed(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //http://localhost:5034/openapi/v1.json
    app.MapOpenApi();


    ////http://localhost:5091/swagger/
    //app.UseSwaggerUI(options =>
    //{
    //    options.SwaggerEndpoint("/openapi/v1.json", "OpenApi V1");
    //});
}

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionHandlingMiddleware>();


app.MapControllers();

app.Run();

