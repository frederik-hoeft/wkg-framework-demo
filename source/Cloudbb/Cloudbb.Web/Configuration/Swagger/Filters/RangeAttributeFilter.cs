using Cloudbb.Web.Configuration.Swagger.Filters.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class RangeAttributeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema writableSchema && context.TryGetAttribute(out RangeAttribute? allowedRange))
        {
            writableSchema.Minimum = allowedRange.Minimum.ToString();
            writableSchema.Maximum = allowedRange.Maximum.ToString();
        }
    }
}
