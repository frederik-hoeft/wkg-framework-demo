namespace Cloudbb.Client.Services.Network;

internal sealed class DefaultHeaderInjectingHandler(IDefaultHeaderInjectorCollection defaultHeaders) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        defaultHeaders.InjectDefaultHeaders(request);
        return base.SendAsync(request, cancellationToken);
    }
}