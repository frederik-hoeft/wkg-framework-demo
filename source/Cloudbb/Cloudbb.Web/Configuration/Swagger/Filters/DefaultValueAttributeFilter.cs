using Cloudbb.Web.Configuration.Swagger.Attributes;
using Cloudbb.Web.Configuration.Swagger.Filters.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class DefaultValueAttributeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema writableSchema && context.TryGetAttribute(out DefaultValueAttribute? defaultValueAttribute))
        {
            string defaultValue = defaultValueAttribute.Value?.ToString() ?? "null";
            writableSchema.Default = defaultValue;
            if (context.HasAttribute<ExampleValueAttribute>())
            {
                writableSchema.Example = defaultValue;
            }
        }
    }
}
