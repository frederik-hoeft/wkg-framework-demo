using Cloudbb.Web.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration;
using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.EntityNamingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.InheritanceValidationPolicies;
using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.PropertyMappingPolicies;
using Wkg.EntityFrameworkCore.Extensions;

namespace Cloudbb.Web.Data;

/// <summary>
/// Primary Entity Framework DbContext for the Cloudbb application.
/// Integrates ASP.NET Core Identity with domain entities and enforces strict mapping policies
/// to ensure explicit entity configuration and maintain architectural boundaries.
/// </summary>
/// <param name="options">Database context configuration options.</param>
/// <param name="modelLoader">Service for discovering and loading entity configurations.</param>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IModelLoader modelLoader) : IdentityDbContext<IdentityUser>(options)
{
    /// <summary>
    /// Configures the database model with strict policies for entity naming, property mapping,
    /// and inheritance validation. Ensures all domain entities follow architectural patterns.
    /// </summary>
    /// <param name="builder">The model builder used to configure entity relationships and constraints.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        base.OnModelCreating(builder);

        builder.LoadModels(modelLoader, modelOptions => modelOptions
            .ConfigurePolicies(policies => policies
                .AddPolicy<EntityNaming>(naming => naming.RequireExplicit())
                .AddPolicy<PropertyMapping>(mapping => mapping.RequireExplicit())
                .AddPolicy<EntityInheritanceValidation>(entity => entity
                    .MustExtend<CloudbbEntity>()
                    .UnlessExtends<ICloudbbConnectionEntity>())));
    }
}
