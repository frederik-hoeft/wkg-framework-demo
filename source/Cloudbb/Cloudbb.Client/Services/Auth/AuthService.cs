using System.Text.Json;
using Cloudbb.Client.Models;
using Cloudbb.Client.Services.Network;

namespace Cloudbb.Client.Services.Auth;

internal sealed class AuthService(IHttpClientFactory httpClientFactory, IJwtAuthenticationState authState, JsonSerializerOptions jsonOptions) 
    : ApiService(httpClientFactory, jsonOptions), IAuthService
{
    public async Task<bool> LoginAsync(LoginRequest request)
    {
        AuthResponse? loginResponse = await PostAsync<LoginRequest, AuthResponse>("api/v1/auth/login", request);
        if (loginResponse is not { IsSuccess: true })
        {
            return false;
        }
        await authState.AuthenticateAsync(loginResponse.Token);
        return true;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        AuthResponse? response = await PostAsync<RegisterRequest, AuthResponse>("api/v1/auth/register", request);
        if (response is not { IsSuccess: true })
        {
            return false;
        }
        await authState.AuthenticateAsync(response.Token);
        return true;
    }

    public Task LogoutAsync() => authState.LogoutAsync();
}