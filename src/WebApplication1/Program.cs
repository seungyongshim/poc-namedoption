using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddOptions<SmtpOption>("SendMail").BindConfiguration("SendMail");
builder.Services.AddKeyedSingleton<SmtpOption>("SendMail", (sp, key) => sp.GetRequiredService<IOptionsFactory<SmtpOption>>().Create(key as string));

builder.Services.AddSingleton<Hello>();

var app = builder.Build();

var sendmailOption = app.Services.GetRequiredKeyedService<SmtpOption>("SendMail");

var hello = app.Services.GetRequiredService<Hello>();

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

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

internal record SmtpOption
{
    public string Url { get; init; }
}


internal class Hello([FromKeyedServices("SendMail")] SmtpOption SmtpOption)
{
    public string Url => SmtpOption.Url;
}
