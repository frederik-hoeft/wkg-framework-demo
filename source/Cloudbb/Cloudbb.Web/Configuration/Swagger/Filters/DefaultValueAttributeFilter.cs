using Cloudbb.Web.Configuration.Swagger.Attributes;
using Cloudbb.Web.Configuration.Swagger.Filters.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Text.Json.Nodes;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class DefaultValueAttributeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema writableSchema && context.TryGetAttribute(out DefaultValueAttribute? defaultValueAttribute))
        {
            writableSchema.Default = ToJsonNode(defaultValueAttribute.Value);
            if (context.HasAttribute<ExampleValueAttribute>())
            {
                writableSchema.Example = ToJsonNode(defaultValueAttribute.Value);
            }
        }
    }

    private static JsonNode? ToJsonNode(object? value) => value switch
    {
        null => null,
        string s => s,
        byte b => b,
        sbyte sb => sb,
        short sh => sh,
        ushort ush => ush,
        int i => i,
        uint ui => ui,
        long l => l,
        ulong ul => ul,
        float f => f,
        double d => d,
        decimal dec => dec,
        bool b => b,
        _ => throw new InvalidOperationException("Unsupported default value type for JSON conversion."),
    };
}
