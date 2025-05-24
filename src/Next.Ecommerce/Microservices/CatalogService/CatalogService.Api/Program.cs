using CatalogService.Application;
using CatalogService.Infrastructure;
using CatalogService.API;

var builder = WebApplication.CreateBuilder(args);

// Register services from different layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Initialize DB
app.SeedDatabase();

// Configure middleware and HTTP pipeline
app.UseHttpsRedirection();
app.UseCustomMiddlewares();
app.UseSwaggerDocs();
app.MapControllers();

app.Run();
