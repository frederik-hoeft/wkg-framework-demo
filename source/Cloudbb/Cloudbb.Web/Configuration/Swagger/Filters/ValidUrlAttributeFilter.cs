using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Wkg.AspNetCore.Validation;
using Wkg.Data.Validation;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class ValidUrlAttributeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.MemberInfo is PropertyInfo pinfo && schema is OpenApiSchema writableSchema)
        {
            if (pinfo.GetCustomAttribute<ValidUrlAttribute>() is not null)
            {
                writableSchema.Pattern = DataValidationService.Url.Pattern;
                writableSchema.Example = "https://example.com";
            }
        }
    }
}
