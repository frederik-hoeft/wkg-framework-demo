namespace Cloudbb.Web.Configuration.Swagger.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
internal sealed class ExampleValueAttribute(string exampleValue) : Attribute
{
    public string ExampleValue => exampleValue;
}
