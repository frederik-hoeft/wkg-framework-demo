using Microsoft.AspNetCore.Identity;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Seeds;

internal sealed class IdentityRoleDataSeed : IDiscoverableModelDataSeed<IdentityRole>
{
    public static IEnumerable<IdentityRole> GetSeedData() =>
    [
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
        }
    ];
}
