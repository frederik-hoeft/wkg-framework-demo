namespace Cloudbb.Client.Services.Auth;

public interface ITokenStore
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task RemoveTokenAsync();
}