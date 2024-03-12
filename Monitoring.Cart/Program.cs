using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Monitoring.SharedKernel.SharedExtensions;
using Monitoring.SharedKernel;

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

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/health", () =>
{

    return Results.Ok("Healthy");
})
.WithName("Health")
.WithOpenApi();

app.MapPost("/cart", ([FromServices] ILogger<Program> logger, [FromBody] ProductDTO productDTO) =>
{
    var traceId = Activity.Current.TraceId.ToString();
    logger.Log(LogLevel.Trace, $"The trace Id is {traceId}!!!");
    return Results.Ok(productDTO);
})
.WithName("AddToCart")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
