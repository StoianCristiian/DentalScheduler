using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DentalScheduler.Client.Auth;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _http;
    
    // Cache pentru user info ca sa nu facem request la fiecare refresh de stare
    private UserInfo? _cachedUserInfo;

    public CustomAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient http)
    {
        _localStorage = localStorage;
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try 
        {
            // Dacă nu avem user info cache-uit, îl cerem de la server
            if (_cachedUserInfo == null)
            {
                var response = await _http.GetAsync("api/auth/manage/info");
                if (response.IsSuccessStatusCode)
                {
                    _cachedUserInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
                }
            }
            
            var claims = new List<Claim>();
            
            if (_cachedUserInfo != null && !string.IsNullOrEmpty(_cachedUserInfo.Email))
            {
                // Setam si Name si Email pentru compatibilitate maxima
                claims.Add(new Claim(ClaimTypes.Name, _cachedUserInfo.Email));
                claims.Add(new Claim(ClaimTypes.Email, _cachedUserInfo.Email));
            }
            
            var identity = new ClaimsIdentity(claims, "Bearer");
            
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch 
        {
            // Daca token-ul e invalid sau request-ul esueaza
            await _localStorage.RemoveItemAsync("authToken");
            _http.DefaultRequestHeaders.Authorization = null;
            _cachedUserInfo = null;
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void MarkUserAsAuthenticated(string token)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Fortam re-evaluarea starii, ceea ce va declansa fetch-ul de info
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkUserAsLoggedOut()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        _cachedUserInfo = null;
        
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private class UserInfo
    {
        public string Email { get; set; } = "";
        public bool IsEmailConfirmed { get; set; }
    }
}
