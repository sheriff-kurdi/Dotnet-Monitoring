using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Monitoring.SharedKernel;
using Monitoring.SharedKernel.SharedExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddServiceDefaults();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("trace-id", Activity.Current.TraceId.ToString());

    // Call the next delegate/middleware in the pipeline.
    await next(context);
});

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return Results.Ok(forecast);
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.MapPost("/add-to-cart", async ([FromServices] ILogger<Program> logger, [FromBody] ProductDTO productDTO) =>
{
    var traceId = Activity.Current.TraceId.ToString();
    logger.Log(LogLevel.Warning, $"The trace Id is {traceId}!!!");

    // Serialize the object to JSON
    string jsonData = JsonSerializer.Serialize(productDTO);

    // Create HttpContent from JSON string
    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

    using (HttpClient client = new HttpClient())
    {
        var response = await client.PostAsync("https://localhost:7254/cart", content);
    }

    return Results.Ok();
})
.WithName("AddToCart")
.WithOpenApi();


app.MapPost("/order", async ([FromServices] ILogger<Program> logger, [FromBody] ProductDTO productDTO) =>
{
    var traceId = Activity.Current.TraceId.ToString();
    logger.Log(LogLevel.Warning, $"The trace Id is {traceId}!!!");

    // Serialize the object to JSON
    string jsonData = JsonSerializer.Serialize(productDTO);

    // Create HttpContent from JSON string
    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

    using (HttpClient client = new HttpClient())
    {
        var response = await client.PostAsync("https://localhost:7093/order", content);
    }

    return Results.Ok();
})
.WithName("Order")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}



