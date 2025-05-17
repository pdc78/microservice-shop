var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var orders = new[]
{
    new Order(1, "John Doe", new[] { "Laptop", "Mouse" }, 1499.99),
    new Order(2, "Jane Smith", new[] { "Smartphone" }, 899.99),
    new Order(3, "Alice Johnson", new[] { "Headphones", "Smartwatch" }, 349.99),
    new Order(4, "Bob Williams", new[] { "Gaming Console", "Controller" }, 599.99)
};

app.MapGet("/api/orders", () =>
{
    return orders;
})
.WithName("GetOrders");

app.Run();

record Order(int Id, string CustomerName, string[] Items, double TotalPrice);
