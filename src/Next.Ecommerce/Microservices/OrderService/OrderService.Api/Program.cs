using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;
using OrderService.Infrastructure.Data;
using Azure.Messaging.ServiceBus;
using OrderService.Infrastructure.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Register repositories and services
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrderDb"));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderProcessingService>();


// Register Azure Service Bus client
builder.Services.AddSingleton(new ServiceBusClient(
 builder.Configuration.GetConnectionString("ServiceBusConnection")));

// Register your publisher
builder.Services.AddScoped<IServiceBusPublisher, AzureServiceBusPublisher>();

// Register the Saga Orchestrator
builder.Services.AddScoped<ISagaOrchestratorService, SagaOrchestratorService>();

// Register other services like IOrderService, etc.
builder.Services.AddScoped<IOrderService, OrderProcessingService>();

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //http://localhost:5242/openapi/v1.json
    app.MapOpenApi();

    //http://localhost:5242/swagger/
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OpenApi V1");
    });
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

