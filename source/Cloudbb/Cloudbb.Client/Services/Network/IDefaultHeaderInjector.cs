namespace Cloudbb.Client.Services.Network;

public interface IDefaultHeaderInjector
{
    string Name { get; }

    void InjectHeaders(HttpRequestMessage request);
}