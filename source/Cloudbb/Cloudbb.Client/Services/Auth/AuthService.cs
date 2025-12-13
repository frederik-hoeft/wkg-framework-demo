using System.Net.Http.Json;
using System.Text.Json;
using Cloudbb.Client.Models;

namespace Cloudbb.Client.Services.Auth;

internal sealed class AuthService
(
    HttpClient httpClient,
    IJwtAuthenticationState authState,
    JsonSerializerOptions jsonOptions
) : IAuthService
{
    public async Task<bool> LoginAsync(LoginRequest request)
    {
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/v1/auth/login", request, jsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }
        using Stream responseContent = await response.Content.ReadAsStreamAsync();
        AuthResponse? loginResponse = await JsonSerializer.DeserializeAsync<AuthResponse>(responseContent, jsonOptions);

        if (loginResponse is not { IsSuccess: true })
        {
            return false;
        }
        await authState.AuthenticateAsync(loginResponse.Token);
        return true;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        using HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("api/v1/auth/register", request);
        if (!responseMessage.IsSuccessStatusCode)
        {
            return false;
        }
        using Stream responseContent = await responseMessage.Content.ReadAsStreamAsync();
        AuthResponse? response = await JsonSerializer.DeserializeAsync<AuthResponse>(responseContent, jsonOptions);

        if (response is not { IsSuccess: true })
        {
            return false;
        }
        await authState.AuthenticateAsync(response.Token);
        return true;
    }

    public Task LogoutAsync() => authState.LogoutAsync();
}