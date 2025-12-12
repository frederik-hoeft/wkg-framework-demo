using Cloudbb.Client.Models;

namespace Cloudbb.Client.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetUsernameAsync();
    Task<string?> GetTokenAsync();
}