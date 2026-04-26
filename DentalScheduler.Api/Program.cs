using DentalScheduler.Application;
using DentalScheduler.Infrastructure;
using DentalScheduler.Infrastructure.Identity;
using DentalScheduler.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Încarcă variabilele din .env dacă există (pentru dezvoltare locală)
var dotenv = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
if (File.Exists(dotenv))
    DotNetEnv.Env.Load(dotenv);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi("v1");

// Configurare CORS pentru a permite accesul din Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.WithOrigins("http://localhost:5289", "https://localhost:7085")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Adăugare strat Application (CQRS, MediatR)
builder.Services.AddApplication();

// Adăugare strat Infrastructură (Database, Repositories, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Activează CORS
app.UseCors("AllowBlazorClient");

app.UseStaticFiles(); // <-- Activeaza fisiere statice pentru pozile de profil

// Activeaza Authentication si Authorization
app.UseAuthentication();
app.UseAuthorization();

// Aplică migrările și seeding-ul la pornire
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Mapăm documentul OpenAPI
    app.MapOpenApi();
    
    // Configurăm Scalar
    app.MapScalarApiReference();
    
    // Redirect automat de la root la Scalar
    app.MapGet("/", () => Results.Redirect("/scalar/v1"));
}

app.UseHttpsRedirection();

app.MapControllers(); 

// Endpoint-uri built-in pentru Identity
app.MapGroup("/api/auth").MapIdentityApi<ApplicationUser>();

app.Run();
