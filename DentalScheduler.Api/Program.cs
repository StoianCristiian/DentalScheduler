using DentalScheduler.Infrastructure;
using DentalScheduler.Infrastructure.Persistance; // Poate fi necesar, in functie de namespace
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("⏳ Caut fisierul .env...");
// Cautam .env urcand in directoare pana la radacina solutiei
var dir = new DirectoryInfo(AppContext.BaseDirectory);
while (dir != null)
{
    var envPath = Path.Combine(dir.FullName, ".env");
    if (File.Exists(envPath))
    {
        Console.WriteLine($"✅ Fisier .env gasit la: {envPath}");
        Env.Load(envPath);
        break;
    }
    dir = dir.Parent;
}

// Adauga configuratia pentru a citi din Environment Variables
builder.Configuration.AddEnvironmentVariables();
Console.WriteLine("⏳ Configurare servicii...");

// Add services to the container.
builder.Services.AddOpenApi();

// Aici facem legătura cu Infrastructure
// Această linie înlocuiește configurarea directă a SQL Server din API
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

Console.WriteLine("⏳ Verific conexiunea la baza de date...");
// TEST CONEXIUNE BAZA DE DATE
using (var scope = app.Services.CreateScope())
{
    try 
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Let's print the connection string to debug (careful, this exposes password in logs temporarily)
        var connString = builder.Configuration.GetConnectionString("DefaultConnection");
        Console.WriteLine($"🔍 Connection String folosit (partial ascuns): {connString?.Replace("ParolaStrong123!", "******")}");

        // Create database if not exists
        try
        {
             Console.WriteLine("⏳ Incerc sa creez baza de date si sa aplic migrarile...");
             await dbContext.Database.MigrateAsync();
             Console.WriteLine("✅ BAZA DE DATE A FOST CREATA/ACTUALIZATA CU SUCCES!");
        }
        catch(Exception ex)
        {
             Console.WriteLine($"⚠️ Avertisment la migrare: {ex.Message}");
        }

        // Try to open connection explicitly to see the exception
        await dbContext.Database.OpenConnectionAsync();
        Console.WriteLine("✅ CONEXIUNE REUSITA LA BAZA DE DATE!");
        await dbContext.Database.CloseConnectionAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ EROARE DETALIATA: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}