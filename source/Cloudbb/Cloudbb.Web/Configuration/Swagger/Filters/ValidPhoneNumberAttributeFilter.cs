using Cloudbb.Web.Configuration.Swagger.Attributes;
using Cloudbb.Web.Configuration.Swagger.Filters.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Wkg.AspNetCore.Validation;
using Wkg.Data.Validation;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class ValidPhoneNumberAttributeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema writableSchema && context.HasAttribute<ValidPhoneNumberAttribute>())
        {
            writableSchema.Pattern = DataValidationService.PhoneNumber.Pattern;
            if (!context.HasAttribute<ExampleValueAttribute>())
            {
                writableSchema.Example = "+1-555-123-4567";
            }
        }
    }
}
