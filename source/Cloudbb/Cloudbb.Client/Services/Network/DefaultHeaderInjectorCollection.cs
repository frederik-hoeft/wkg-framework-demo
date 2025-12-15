using System.Collections;
using System.Collections.Concurrent;

namespace Cloudbb.Client.Services.Network;

internal sealed class DefaultHeaderInjectorCollection : IDefaultHeaderInjectorCollection
{
    private readonly ConcurrentDictionary<string, IDefaultHeaderInjector> _injectors = new();

    public void InjectDefaultHeaders(HttpRequestMessage request)
    {
        foreach (IDefaultHeaderInjector injector in _injectors.Values)
        {
            Console.WriteLine($"Injecting headers using injector: {injector.Name} into {request.Method} {request.RequestUri}");
            injector.InjectHeaders(request);
        }
    }

    public bool TryAdd(IDefaultHeaderInjector injector) => _injectors.TryAdd(injector.Name, injector);

    public bool TryRemove(string name) => _injectors.TryRemove(name, out _);

    public IEnumerator<IDefaultHeaderInjector> GetEnumerator() => _injectors.Values.GetEnumerator();

    public void Clear() => _injectors.Clear();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void AddOrUpdate(IDefaultHeaderInjector injector) => _injectors.AddOrUpdate(injector.Name, injector, (_, _) => injector);
}