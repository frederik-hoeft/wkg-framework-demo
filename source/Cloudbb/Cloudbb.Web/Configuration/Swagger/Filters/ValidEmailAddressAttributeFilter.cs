using Cloudbb.Web.Configuration.Swagger.Attributes;
using Cloudbb.Web.Configuration.Swagger.Filters.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Wkg.AspNetCore.Validation;
using Wkg.Data.Validation;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class ValidEmailAddressAttributeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema writableSchema && context.HasAttribute<ValidEmailAddressAttribute>())
        {
            writableSchema.Pattern = DataValidationService.EmailAddress.Pattern;
            if (!context.HasAttribute<ExampleValueAttribute>())
            {
                writableSchema.Example = "mail@example.com";
            }
        }
    }
}
