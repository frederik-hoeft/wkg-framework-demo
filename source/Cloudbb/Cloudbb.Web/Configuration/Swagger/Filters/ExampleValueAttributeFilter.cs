using Cloudbb.Web.Configuration.Swagger.Attributes;
using Cloudbb.Web.Configuration.Swagger.Filters.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class ExampleValueAttributeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema writableSchema 
            && context.TryGetAttribute(out ExampleValueAttribute? exampleValueAttribute) 
            && exampleValueAttribute is { ExampleValue: { Length: > 0 } exampleValue })
        {
            writableSchema.Example = exampleValue;
        }
    }
}
