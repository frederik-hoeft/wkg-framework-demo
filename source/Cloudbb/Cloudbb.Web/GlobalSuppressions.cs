// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;
using static GlobalSuppressions;

[assembly: SuppressMessage("Maintainability", 
    "CA1515:Consider making public types internal",
    Justification = "API project needs to expose controllers",
    Scope = "namespaceanddescendants",
    Target = "~N:Cloudbb.Web")]

// Suppressions for EF Core generated code
[assembly: SuppressMessage("Style", "IDE0161:Convert to file-scoped namespace", Justification = EF_GENERATED_CODE, Scope = "namespaceanddescendants", Target = "~N:Cloudbb.Web.Data.Migrations")]
[assembly: SuppressMessage("Style", "IDE0053:Use expression body for lambda expression", Justification = EF_GENERATED_CODE, Scope = "namespaceanddescendants", Target = "~N:Cloudbb.Web.Data.Migrations")]
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = EF_GENERATED_CODE, Scope = "namespaceanddescendants", Target = "~N:Cloudbb.Web.Data.Migrations")]
[assembly: SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = EF_GENERATED_CODE, Scope = "namespaceanddescendants", Target = "~N:Cloudbb.Web.Data.Migrations")]

file static class GlobalSuppressions
{
    public const string EF_GENERATED_CODE = "Generated entity framework migration code";
}