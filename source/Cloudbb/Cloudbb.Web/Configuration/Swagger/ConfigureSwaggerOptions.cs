using Asp.Versioning.ApiExplorer;
using Cloudbb.Web.Configuration.Swagger.Filters;
using Cloudbb.Web.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cloudbb.Web.Configuration.Swagger;

internal sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration) : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        // add swagger document for every API version discovered
        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }
        string? processPath = configuration["ProgramPath"];
        if (string.IsNullOrEmpty(processPath))
        {
            processPath = Path.GetProcessPath();
        }
        string fullPath = Path.Combine(processPath, "Cloudbb.Web.xml");
        options.IncludeXmlComments(fullPath, includeControllerXmlComments: true);
        options.SupportNonNullableReferenceTypes();
        options.SchemaFilter<AllowedValuesAttributeFilter>();
        options.SchemaFilter<DefaultValueAttributeFilter>();
        options.SchemaFilter<ExampleValueAttributeFilter>();
        options.SchemaFilter<RangeAttributeFilter>();
        options.SchemaFilter<TimeSpanTypeFilter>();
        options.SchemaFilter<ValidUrlAttributeFilter>();
        options.SchemaFilter<ValidEmailAddressAttributeFilter>();
        options.SchemaFilter<ValidPhoneNumberAttributeFilter>();
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            In = ParameterLocation.Header,
            Description = "Please provide a previously obtained JWT token.",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "bearer"
        });
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document), []
            }
        });
    }

    public void Configure(string? name, SwaggerGenOptions options) => Configure(options);

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        OpenApiInfo info = new()
        {
            Title = "Cloudbb API",
            Version = description.ApiVersion.ToString()
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }

        return info;
    }
}