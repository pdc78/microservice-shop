using CatalogService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;
using OrderService.Infrastructure.Data;
using Azure.Messaging.ServiceBus;
using OrderService.Infrastructure.ServiceBus;
using OrderService.Application;
using OrderService.Domain.Events;

var builder = WebApplication.CreateBuilder(args);

// Register repositories and services
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrderDb"));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddSingleton<ServiceBusClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("ServiceBus");

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Missing Service Bus connection string");

    var clientOptions = new ServiceBusClientOptions
    {
        RetryOptions = new ServiceBusRetryOptions
        {
            Mode = ServiceBusRetryMode.Exponential,
            MaxRetries = 3,
            Delay = TimeSpan.FromSeconds(0.5),
            MaxDelay = TimeSpan.FromSeconds(10)
        }
    };

    return new ServiceBusClient(connectionString, clientOptions);
});

builder.Services.AddScoped<IServiceBusPublisher, AzureServiceBusPublisher>();


// Register other services like IOrderService, etc.

builder.Services.AddScoped<IOrderService>(sp =>
{
    var repository = sp.GetRequiredService<IOrderRepository>();
    var publisher = sp.GetRequiredService<IServiceBusPublisher>();
    var logger = sp.GetRequiredService<ILogger<OrderProcessingService>>();
    var configuration = sp.GetRequiredService<IConfiguration>();

    var topicName = configuration["Topics:Order"];
    if (string.IsNullOrWhiteSpace(topicName))
        throw new InvalidOperationException("Missing Order topic configuration");

    return new OrderProcessingService(repository, publisher, topicName, logger);
});


var subscriptions = builder.Services.Configure<SagaSubscriptionOptions>(
 builder.Configuration.GetSection("SagaSubscriptionOptions"));



// Register the Saga Orchestrator
builder.Services.AddSingleton<IHostedService, SagaOrchestratorService>();

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

