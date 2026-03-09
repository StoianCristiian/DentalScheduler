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
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            if (_cachedUserInfo == null)
            {
                var response = await _http.GetAsync("api/auth/manage/info");
                if (response.IsSuccessStatusCode)
                    _cachedUserInfo = await response.Content.ReadFromJsonAsync<UserInfo>();
            }

            var claims = new List<Claim>();

            if (_cachedUserInfo != null && !string.IsNullOrEmpty(_cachedUserInfo.Email))
            {
                claims.Add(new Claim(ClaimTypes.Name, _cachedUserInfo.Email));
                claims.Add(new Claim(ClaimTypes.Email, _cachedUserInfo.Email));
            }

            // Incarcam claims stocate din JWT (roluri, sub, etc.)
            var storedClaims = await _localStorage.GetItemAsync<List<StoredClaim>>("userClaims");
            if (storedClaims != null)
            {
                foreach (var c in storedClaims)
                {
                    // Identity Bearer pune rolurile in "role" - le mapam la ClaimTypes.Role
                    // pentru ca AuthorizeView Roles="..." sa functioneze corect
                    if (c.Type == "role")
                        claims.Add(new Claim(ClaimTypes.Role, c.Value));
                    else
                        claims.Add(new Claim(c.Type, c.Value));
                }
            }

            var identity = new ClaimsIdentity(claims, "Bearer");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("userClaims");
            await _localStorage.RemoveItemAsync("userId");
            _http.DefaultRequestHeaders.Authorization = null;
            _cachedUserInfo = null;
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public void MarkUserAsAuthenticated(string token, List<StoredClaim> claims)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _cachedUserInfo = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void MarkUserAsLoggedOut()
    {
        _http.DefaultRequestHeaders.Authorization = null;
        _cachedUserInfo = null;
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
    }

    private class UserInfo
    {
        public string Email { get; set; } = "";
        public bool IsEmailConfirmed { get; set; }
    }
}

public class StoredClaim
{
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
}
