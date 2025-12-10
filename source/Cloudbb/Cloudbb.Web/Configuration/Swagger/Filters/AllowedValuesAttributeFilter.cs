using Cloudbb.Web.Configuration.Swagger.Filters.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class AllowedValuesAttributeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema writableSchema && context.TryGetAttribute(out AllowedValuesAttribute? allowedValues))
        {
            writableSchema.Enum =
            [
                .. allowedValues.Values.Select(static item => item?.ToString() ?? "null")
            ];
        }
    }
}
