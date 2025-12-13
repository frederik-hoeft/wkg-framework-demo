namespace Cloudbb.Client.Services;

public interface IJwtAuthenticationState
{
    Task AuthenticateAsync(string jwtToken);

    Task LogoutAsync();
}
