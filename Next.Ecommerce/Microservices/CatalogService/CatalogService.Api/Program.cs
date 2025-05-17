var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //http://localhost:5034/openapi/v1.json
    app.MapOpenApi();

    
    //http://localhost:5091/swagger/
    // app.UseSwaggerUI(options =>
    // {
    //     options.SwaggerEndpoint("/openapi/v1.json", "OpenApi V1");
    // });
}

app.UseHttpsRedirection();

// http://localhost:5096/api/catalog/products
var products = new[]
{
    new Product(1, "Laptop", "High-performance laptop", 1299.99),
    new Product(2, "Smartphone", "Latest model smartphone", 899.99),
    new Product(3, "Headphones", "Noise-canceling headphones", 199.99),
    new Product(4, "Smartwatch", "Feature-rich smartwatch", 249.99),
    new Product(5, "Gaming Console", "Next-gen gaming console", 499.99),
    new Product(6, "Gaming Console 2", "Next-gen gaming console", 499.99)
};

// Ensure the endpoint matches the Ocelot configuration
app.MapGet("/api/catalog/products", () =>
{
    return products;
})
.WithName("GetProducts");

app.Run();

record Product(int Id, string Name, string Description, double Price);