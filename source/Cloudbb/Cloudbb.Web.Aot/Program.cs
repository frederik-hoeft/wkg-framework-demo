using Cloudbb.Web.Aot;
using Cloudbb.Web.Aot.Services.Versioning.Default;
using Cloudbb.Web.Services.Versioning;
using System.Text.Json.Serialization;
using Wkg.Versioning;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IVersionProvider, CloudbbWebAotVersionProvider>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

RouteGroupBuilder v1Api = app.MapGroup("/api/v1");

v1Api.MapGet("/version", GetVersionResponse (IVersionProvider versionProvider) =>
    {
        DeploymentVersionInfo versionInfo = versionProvider.GetVersionInfo();
        GetVersionResponse response = new(versionInfo.VersionString, versionInfo.BuildDateUtc, versionInfo.IsPreRelease);
        return response;
    })
    .WithName("GetVersion");

app.Run();

[JsonSerializable(typeof(GetVersionResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;