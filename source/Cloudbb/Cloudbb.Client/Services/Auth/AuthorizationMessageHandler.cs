using Microsoft.AspNetCore.Components;
using System.Net;

namespace Cloudbb.Client.Services.Auth;

internal sealed class AuthorizationMessageHandler(IJwtAuthenticationState authState, NavigationManager navigationManager) : DelegatingHandler
{
    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Clear the token and redirect to login
            await authState.LogoutAsync();
            navigationManager.NavigateTo("/login", forceLoad: true);
        }

        return response;
    }
}