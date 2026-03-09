using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DentalScheduler.Client;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using DentalScheduler.Client.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Schimbam BaseAddress sa pointeze catre API, nu catre Client
// Atentie: Asigura-te ca API-ul ruleaza pe acest port (5062 este cel HTTP din logurile tale)
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5062") });

// Adaugam servicii pentru Autentificare
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

await builder.Build().RunAsync();