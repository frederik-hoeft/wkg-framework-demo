using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Cloudbb.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cloudbb.Client.Services;

public sealed class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/auth/login", request);
            
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                LoginResponse? loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (loginResponse != null)
                {
                    await _localStorage.SetItemAsync("authToken", loginResponse.Token);
                    await _localStorage.SetItemAsync("username", loginResponse.Username);
                    await _localStorage.SetItemAsync("email", loginResponse.Email);
                    
                    ((JwtAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginResponse.Username);
                    
                    return true;
                }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/v1/auth/register", new
            {
                Username = request.Username,
                Email = request.Email,
                Password = request.Password
            });
            
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("username");
        await _localStorage.RemoveItemAsync("email");
        
        ((JwtAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        string? token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetUsernameAsync()
    {
        return await _localStorage.GetItemAsync<string>("username");
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }
}