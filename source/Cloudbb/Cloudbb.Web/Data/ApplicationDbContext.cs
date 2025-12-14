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

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IModelLoader modelLoader) : IdentityDbContext<IdentityUser>(options)
{
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
