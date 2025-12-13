namespace Cloudbb.Client.Services.Auth;

public interface IJwtAuthenticationState
{
    Task AuthenticateAsync(string jwtToken);

    Task LogoutAsync();
}
