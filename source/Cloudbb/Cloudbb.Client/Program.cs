using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Cloudbb.Client;
using MudBlazor.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text.Json;
using Cloudbb.Client.Services.Auth;
using Cloudbb.Client.Services.TimeZone;
using Cloudbb.Client.Services.Posts;
using Cloudbb.Client.Services.Comments;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API base URL
string apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7156"; // Default to API port

// Add authorization message handler
builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped(sp =>
{
    AuthorizationMessageHandler handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    HttpClient httpClient = new(handler)
    {
        BaseAddress = new Uri(apiBaseUrl)
    };
    return httpClient;
});

// Add MudBlazor services
builder.Services.AddMudServices();

// Add local storage
builder.Services.AddBlazoredLocalStorage();

// Add authentication services
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
});
builder.Services.AddScoped<ITokenStore, LocalStorageTokenStore>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<IJwtAuthenticationState, JwtAuthenticationStateAccessor>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Add timezone service
builder.Services.AddScoped<ITimeZoneService, LocalStorageTimeZoneService>();

// Add post and comment services
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();

await builder.Build().RunAsync();
