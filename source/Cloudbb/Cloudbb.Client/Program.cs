using Blazored.LocalStorage;
using Cloudbb.Client;
using Cloudbb.Client.Services;
using Cloudbb.Client.Services.Auth;
using Cloudbb.Client.Services.Comments;
using Cloudbb.Client.Services.Network;
using Cloudbb.Client.Services.Posts;
using Cloudbb.Client.Services.TimeZone;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure API base URL
string apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7156"; // Default to API port

// Add HTTP client with authorization and default header handlers
builder.Services.AddSingleton<IDefaultHeaderInjectorCollection, DefaultHeaderInjectorCollection>();
builder.Services.AddTransient<AuthorizationMessageHandler>();
builder.Services.AddTransient<DefaultHeaderInjectingHandler>();
builder.Services.AddHttpClient("Cloudbb.Web.Client", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthorizationMessageHandler>()
    .AddHttpMessageHandler<DefaultHeaderInjectingHandler>();

// Add MudBlazor services
builder.Services.AddMudServices();

// Add local storage
builder.Services.AddBlazoredLocalStorage();

// Add authentication services
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters =
    {
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
    }
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
