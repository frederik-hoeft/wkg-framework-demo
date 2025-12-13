namespace Cloudbb.Client.Services;

public interface ITokenStore
{
    Task<string?> GetTokenAsync();
    Task SetTokenAsync(string token);
    Task RemoveTokenAsync();
}