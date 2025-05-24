using CatalogService.API.Data;
using CatalogService.Api.Middlewares;
using CatalogService.Infrastructure.Data;

namespace CatalogService.API;

public static class WebApplicationExtensions
{
    public static void SeedDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        DbInitializer.Seed(context);
    }
    
    public static void UseCustomMiddlewares(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    public static void UseSwaggerDocs(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "OpenApi V1");
            });
        }
    }
}
