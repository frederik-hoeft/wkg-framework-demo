using Cloudbb.Web.Configuration.Swagger.Attributes;
using Cloudbb.Web.Configuration.Swagger.Filters.Extensions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Cloudbb.Web.Configuration.Swagger.Filters;

internal sealed class TimeSpanTypeFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is OpenApiSchema writableSchema && !context.HasAttribute<ExampleValueAttribute>()
            && (context.MemberInfo is PropertyInfo prop && (prop.PropertyType == typeof(TimeSpan) || prop.PropertyType == typeof(TimeSpan?))
                || context.ParameterInfo is ParameterInfo param && (param.ParameterType == typeof(TimeSpan) || param.ParameterType == typeof(TimeSpan?))))
        {
            writableSchema.Example = TimeSpan.Zero.ToString();
        }
    }
}
