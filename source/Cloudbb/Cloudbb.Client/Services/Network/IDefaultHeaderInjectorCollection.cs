namespace Cloudbb.Client.Services.Network;

public interface IDefaultHeaderInjectorCollection : IEnumerable<IDefaultHeaderInjector>
{
    void InjectDefaultHeaders(HttpRequestMessage request);

    bool TryAdd(IDefaultHeaderInjector injector);

    void AddOrUpdate(IDefaultHeaderInjector injector);

    bool TryRemove(string name);

    void Clear();
}