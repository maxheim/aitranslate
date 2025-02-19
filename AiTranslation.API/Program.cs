using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.MapGet("/translate", async () =>
{
    var result = "Hello World";
    
    using (var client = new HttpClient())
    {
        try
        {
            var request = new PromptRequest(
                "llama3.2:1B",
                "Bitte übersetze den folgenden Text ins Englische: Mary hatte ein kleines Lamm.", 
                false);
            
            string jsonRequest = JsonSerializer.Serialize(request);

            StringContent serializedRequest = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync("http://ollama:11434/api/generate", serializedRequest);
            var mappedResponse = await response.Content.ReadFromJsonAsync<PromptResponse>();
            
            return Results.Ok(new
            {
                mappedResponse
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new
            {
                message = ex.Message
            });
        }
        
    }

    
});


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

class PromptRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; }

    [JsonPropertyName("stream")]
    public bool Stream { get; set; }

    public PromptRequest(string model, string prompt, bool stream)
    {
        Model = model;
        Prompt = prompt;
        Stream = stream;
    }
}

class PromptResponse
{
    public string Response { get; set; }
    public string Model { get; set; }

    public PromptResponse(string response, string model)
    {
        Response = response;
        Model = model;
    }
}