using DentalScheduler.Application;
using DentalScheduler.Infrastructure;
using DentalScheduler.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Încarcă variabilele din .env dacă există (pentru dezvoltare locală)
var root = Directory.GetCurrentDirectory();
var dotenv = Path.Combine(root, "..", ".env");
if (File.Exists(dotenv))
{
    DotNetEnv.Env.Load(dotenv);
}

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers(); 
builder.Services.AddOpenApi("v1"); // <-- MODIFICAT: Specificam un nume pentru document ("v1")

// Adăugare strat Application (CQRS, MediatR)
builder.Services.AddApplication();

// Adăugare strat Infrastructură (Database, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Aplică migrarile automat la pornire
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Mapăm documentul OpenAPI specificat mai sus
    app.MapOpenApi("/openapi/v1.json"); 
    
    // Configurăm Scalar să citească exact acest document
    app.MapScalarApiReference(options => 
    {
        options.WithOpenApiRoutePattern("/openapi/v1.json");
    });
    
    // Redirect automat
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
}

app.UseHttpsRedirection();

app.MapControllers(); // <-- LINIE NOUA: Activează rutele din controllere

app.Run();
