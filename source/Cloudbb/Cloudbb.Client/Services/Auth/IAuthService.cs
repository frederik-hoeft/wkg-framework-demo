using Cloudbb.Client.Models;

namespace Cloudbb.Client.Services.Auth;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
}