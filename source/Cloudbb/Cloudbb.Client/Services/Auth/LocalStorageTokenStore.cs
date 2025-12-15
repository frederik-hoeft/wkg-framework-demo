using Blazored.LocalStorage;

namespace Cloudbb.Client.Services;

internal sealed class LocalStorageTokenStore(ILocalStorageService localStorage) : ITokenStore
{
    private const string TOKEN_KEY = "authToken";

    public async Task<string?> GetTokenAsync() => await localStorage.GetItemAsync<string>(TOKEN_KEY);

    public async Task SetTokenAsync(string token) => await localStorage.SetItemAsync(TOKEN_KEY, token);

    public async Task RemoveTokenAsync() => await localStorage.RemoveItemAsync(TOKEN_KEY);
}