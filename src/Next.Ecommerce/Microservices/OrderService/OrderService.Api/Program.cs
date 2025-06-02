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


builder.Services.AddScoped<IServiceBusPublisher, AzureServiceBusPublisher>();

// Register Azure Service Bus client
// builder.Services.AddSingleton(new ServiceBusClient(
//  builder.Configuration.GetConnectionString("ServiceBusConnection")));


builder.Services.AddSingleton<ServiceBusClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("ServiceBus");

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Missing Service Bus connection string");

    return new ServiceBusClient(connectionString);
});



// Register other services like IOrderService, etc.
builder.Services.AddScoped<IOrderService, OrderProcessingService>();

// Register the Saga Orchestrator
builder.Services.AddSingleton<IHostedService, SagaOrchestratorService>();
// builder.Services.AddScoped<ISagaOrchestratorService, SagaOrchestratorService>();

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

