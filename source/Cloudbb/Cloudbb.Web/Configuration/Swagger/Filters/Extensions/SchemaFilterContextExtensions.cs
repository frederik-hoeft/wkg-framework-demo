using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Cloudbb.Web.Configuration.Swagger.Filters.Extensions;

internal static class SchemaFilterContextExtensions
{
    extension(SchemaFilterContext context)
    {
        public bool TryGetAttribute<T>([NotNullWhen(true)] out T? attribute) where T : Attribute
        {
            if (context.MemberInfo is { } memberInfo && ((attribute = memberInfo.GetCustomAttribute<T>()) != null)
                || context.ParameterInfo is { } paramInfo && ((attribute = paramInfo.GetCustomAttribute<T>()) != null))
            {
                return true;
            }
            attribute = null;
            return false;
        }

        public bool HasAttribute<T>() where T : Attribute => context.TryGetAttribute(out T? _);
    }
}
