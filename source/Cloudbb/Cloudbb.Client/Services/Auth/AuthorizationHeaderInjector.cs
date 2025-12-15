using Cloudbb.Client.Services.Network;
using System.Net.Http.Headers;

namespace Cloudbb.Client.Services.Auth;

internal sealed class AuthorizationHeaderInjector(string scheme, string? parameters) : IDefaultHeaderInjector
{
    private readonly AuthenticationHeaderValue _authenticationHeaderValue = new(scheme, parameters);

    public string Name => "Authorization";

    public void InjectHeaders(HttpRequestMessage request) => request.Headers.Authorization = _authenticationHeaderValue;
}