using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

public abstract class CloudbbEntity : IReflectiveBaseModelConfiguration<CloudbbEntity>
{
    public Guid Id { get; set; }

    static void IReflectiveBaseModelConfiguration<CloudbbEntity>.ConfigureBaseModel<TChildClass>(EntityTypeBuilder<TChildClass> self)
    {
        ArgumentNullException.ThrowIfNull(self);

        self.Property(my => my.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("uuidv7()")
            .IsRequired();
    }
}
