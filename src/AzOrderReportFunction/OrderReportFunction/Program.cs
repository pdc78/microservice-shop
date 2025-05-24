using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderReportFunction.Mock.Services;
using OrderReportFunction.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();


builder.Services.AddScoped<IOrderService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var useMock = config.GetValue<bool>("UseMockData");

    return useMock
    ? new MockOrderService()
    : new OrderService(config);
});

builder.Services.AddScoped<IEmailService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var useMock = config.GetValue<bool>("UseMockData");

    return useMock
    ? new MockEmailService()
    : new SendGridEmailService(config);
});

builder.Services.AddScoped<IPdfGenerator, PdfGenerator>();

builder.Build().Run();
