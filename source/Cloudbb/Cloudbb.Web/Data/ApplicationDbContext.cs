using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.EntityNamingPolicies;
using Wkg.EntityFrameworkCore.Configuration.Policies.Defaults.PropertyMappingPolicies;
using Wkg.EntityFrameworkCore.Extensions;

namespace Cloudbb.Web.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // TODO: use source generated model discovery once Wkg.EntityFrameworkCore v10.0 is published (currently expired NuGet API keys)
        builder.LoadReflectiveModels(modelOptions => modelOptions
            .ConfigurePolicies(policies => policies
                .AddPolicy<EntityNaming>(naming => naming.RequireExplicit())
                .AddPolicy<PropertyMapping>(mapping => mapping.RequireExplicit())));

        // TODO: use Wkg.EntityFrameworkCore v10.0 data seeding
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "019ae512-ae98-7973-8de5-7e654298ec4b",
                ConcurrencyStamp = "828439b8-c96e-48c1-860d-62076f394c76",
                Name = "admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole
            {
                Id = "019ae512-d047-7708-b68e-a0e24a6ced56",
                ConcurrencyStamp = "bc86bddf-c568-4f39-91a2-330595964e90",
                Name = "user",
                NormalizedName = "USER"
            });
    }
}
