using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Base entity class for all domain entities in the Cloudbb forum system.
/// Provides common properties and configuration patterns for consistent data modeling.
/// </summary>
public abstract class CloudbbEntity : IDiscoverableBaseModelConfiguration<CloudbbEntity>
{
    /// <summary>
    /// Unique identifier for the entity. Used as the primary key across all domain entities.
    /// </summary>
    public Guid Id { get; set; }

    static void IBaseModelConfiguration<CloudbbEntity>.ConfigureBaseModel<TChildClass>(EntityTypeBuilder<TChildClass> self)
    {
        ArgumentNullException.ThrowIfNull(self);

        self.Property(my => my.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("uuidv7()")
            .IsRequired();
    }
}
