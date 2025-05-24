using BasketService.Application;
using BasketService.Infrastructure;
using BasketService.API;

var builder = WebApplication.CreateBuilder(args);

// Modular service registrations
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseSwaggerDocs();
app.UseHttpsRedirection();

app.MapControllers();
app.Run();