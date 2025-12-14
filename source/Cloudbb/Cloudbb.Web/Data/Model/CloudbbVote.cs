using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

public abstract class CloudbbVote : ICloudbbConnectionEntity, IDiscoverableBaseModelConfiguration<CloudbbVote>
{
    public Guid UserId { get; set; }

    public int Value { get; set; }

    public CloudbbUser User { get; set; } = null!;

    static void IBaseModelConfiguration<CloudbbVote>.ConfigureBaseModel<TChildClass>(EntityTypeBuilder<TChildClass> self)
    {
        ArgumentNullException.ThrowIfNull(self);

        self.Property(my => my.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        self.Property(my => my.Value)
            .HasColumnName("value")
            .HasColumnType("smallint")
            .IsRequired();

        self.HasOne(my => my.User)
            .WithMany()
            .HasForeignKey(my => my.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}