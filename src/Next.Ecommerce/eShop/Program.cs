using eShop.ApiClients;
using eShop.Services;
using eShop.Services.Interfaces;
using eShop.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IHttpStrategy, HttpClientStrategy>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["API_GATEWAY_URL"]);
});

builder.Services.AddScoped<IBasketApiClient, BasketApiClient>();
builder.Services.AddScoped<ICatalogApiClient, CatalogApiClient>();
builder.Services.AddScoped<IOrderApiClient, OrderApiClient>();


builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IBasketService, BasketService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
