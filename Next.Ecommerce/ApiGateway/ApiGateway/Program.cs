using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load Ocelot configuration
builder.Configuration.AddJsonFile("OcelotConfigurations/ocelot.micoservice.json", optional: false, reloadOnChange: true);

// Register Ocelot services
builder.Services.AddOcelot();

var app = builder.Build();

// Enable Ocelot middleware to handle API routing
app.UseOcelot().Wait();

app.Run();


// Invoke-RestMethod -Uri "http://localhost:5096/api/catalog/products" -Method Get
// Invoke-RestMethod -Uri "http://localhost:5096/api/orders" -Method Get       