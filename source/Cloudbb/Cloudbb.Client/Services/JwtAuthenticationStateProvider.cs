using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cloudbb.Client.Services;

public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly HttpClient _httpClient;
    private AuthenticationState _anonymous;

    public JwtAuthenticationStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
        _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string? token = await _localStorage.GetItemAsync<string>("authToken");
            
            if (string.IsNullOrEmpty(token))
            {
                return _anonymous;
            }

            // Parse JWT token to extract claims
            ClaimsPrincipal user = CreateClaimsFromJwt(token);
            
            if (user.Identity?.IsAuthenticated == true)
            {
                // Add the token to the HTTP client headers for API calls
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return new AuthenticationState(user);
            }
            
            return _anonymous;
        }
        catch
        {
            return _anonymous;
        }
    }

    public void MarkUserAsAuthenticated(string username)
    {
        ClaimsIdentity authenticatedUser = new(new[]
        {
            new Claim(ClaimTypes.Name, username)
        }, "jwt");

        ClaimsPrincipal claimsPrincipal = new(authenticatedUser);
        Task<AuthenticationState> authState = Task.FromResult(new AuthenticationState(claimsPrincipal));
        
        NotifyAuthenticationStateChanged(authState);
    }

    public void MarkUserAsLoggedOut()
    {
        // Remove authorization header
        _httpClient.DefaultRequestHeaders.Authorization = null;
        
        Task<AuthenticationState> authState = Task.FromResult(_anonymous);
        NotifyAuthenticationStateChanged(authState);
    }

    private static ClaimsPrincipal CreateClaimsFromJwt(string jwt)
    {
        try
        {
            // Simple JWT parsing - extract the payload
            string[] tokenParts = jwt.Split('.');
            if (tokenParts.Length != 3)
            {
                return new ClaimsPrincipal(new ClaimsIdentity());
            }

            string payload = tokenParts[1];
            
            // Pad the payload if necessary
            int mod = payload.Length % 4;
            if (mod != 0)
            {
                payload += new string('=', 4 - mod);
            }

            byte[] payloadBytes = Convert.FromBase64String(payload);
            string payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
            
            using JsonDocument doc = JsonDocument.Parse(payloadJson);
            JsonElement root = doc.RootElement;

            List<Claim> claims = new();

            // Extract standard claims
            if (root.TryGetProperty("sub", out JsonElement sub))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.GetString() ?? ""));
            }

            if (root.TryGetProperty("unique_name", out JsonElement uniqueName))
            {
                claims.Add(new Claim(ClaimTypes.Name, uniqueName.GetString() ?? ""));
            }

            if (root.TryGetProperty("email", out JsonElement email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email.GetString() ?? ""));
            }

            if (root.TryGetProperty("role", out JsonElement role))
            {
                if (role.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement roleElement in role.EnumerateArray())
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleElement.GetString() ?? ""));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.GetString() ?? ""));
                }
            }

            ClaimsIdentity identity = new(claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}