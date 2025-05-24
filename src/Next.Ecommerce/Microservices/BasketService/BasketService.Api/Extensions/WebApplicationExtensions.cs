namespace BasketService.API;

public static class WebApplicationExtensions
{
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
