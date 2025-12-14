using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class AllowedValuesAttributeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo is PropertyInfo pinfo && schema is OpenApiSchema writableSchema)
        {
            if (pinfo.GetCustomAttribute<AllowedValuesAttribute>() is AllowedValuesAttribute allowedValues)
            {
                writableSchema.Enum =
                [
                    .. allowedValues.Values.Select(static item => item?.ToString() ?? "null")
                ];
            }
        }
    }
}
