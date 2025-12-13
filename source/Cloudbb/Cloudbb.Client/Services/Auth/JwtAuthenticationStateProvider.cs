using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Cloudbb.Client.Services.Auth;

internal sealed class JwtAuthenticationStateProvider(ITokenStore tokenStore, HttpClient httpClient) : AuthenticationStateProvider
{
    private readonly AuthenticationState _anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            string? token = await tokenStore.GetTokenAsync();
            
            if (string.IsNullOrEmpty(token))
            {
                return _anonymous;
            }

            // Parse JWT token to extract claims
            ClaimsPrincipal user = CreateClaimsFromJwt(token);
            
            if (user.Identity?.IsAuthenticated == true)
            {
                // Add the token to the HTTP client headers for API calls
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                return new AuthenticationState(user);
            }
            
            return _anonymous;
        }
        catch
        {
            return _anonymous;
        }
    }

    public async Task AuthenticateAsync(string jwtToken)
    {
        await tokenStore.SetTokenAsync(jwtToken);
        // Parse JWT token to extract claims
        ClaimsPrincipal claimsPrincipal = CreateClaimsFromJwt(jwtToken);
        Task<AuthenticationState> authState = Task.FromResult(new AuthenticationState(claimsPrincipal));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task LogoutAsync()
    {
        await tokenStore.RemoveTokenAsync();
        // Remove authorization header
        httpClient.DefaultRequestHeaders.Authorization = null;
        Task<AuthenticationState> authState = Task.FromResult(_anonymous);
        NotifyAuthenticationStateChanged(authState);
    }

    private static ClaimsPrincipal CreateClaimsFromJwt(string jwt)
    {
        JwtSecurityToken jwtToken = new(jwt);
        return new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims, "jwt"));
    }
}