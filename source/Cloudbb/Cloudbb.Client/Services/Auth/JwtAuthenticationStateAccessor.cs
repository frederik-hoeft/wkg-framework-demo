using Microsoft.AspNetCore.Components.Authorization;

namespace Cloudbb.Client.Services.Auth;

internal sealed class JwtAuthenticationStateAccessor(AuthenticationStateProvider authenticationStateProvider) : IJwtAuthenticationState
{
    private readonly JwtAuthenticationStateProvider _jwtAuthState = (JwtAuthenticationStateProvider)authenticationStateProvider;

    public Task AuthenticateAsync(string jwtToken) => _jwtAuthState.AuthenticateAsync(jwtToken);

    public Task LogoutAsync() => _jwtAuthState.LogoutAsync();
}
