using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace Cloudbb.Client.Services.Network;

internal abstract class ApiService(IHttpClientFactory httpClientFactory, JsonSerializerOptions jsonOptions)
{
    protected HttpClient HttpClient { get; } = httpClientFactory.CreateClient(NamedClient.Default);

    protected IHttpClientFactory HttpClientFactory => httpClientFactory;

    protected JsonSerializerOptions JsonOptions => jsonOptions;

    protected Task<TResponse?> PostAsync<TRequest, TResponse>(string requestUri, TRequest request, TResponse? defaultValue = default) => ResultOrDefaultAsync(async () =>
    {
        using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(requestUri, request, jsonOptions);
        Debugger.Break();
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(jsonOptions);
    }, defaultValue);

    protected Task<bool> PostAsync<TRequest>(string requestUri, TRequest request) => ResultOrDefaultAsync(async () =>
    {
        using HttpResponseMessage response = await HttpClient.PostAsJsonAsync(requestUri, request, jsonOptions);
        return response.IsSuccessStatusCode;
    }, defaultValue: false);

    protected static async Task<TResult?> ResultOrDefaultAsync<TResult>(Func<Task<TResult>> func, TResult? defaultValue = default)
    {
        try
        {
            return await func();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return defaultValue;
        }
    }
}
