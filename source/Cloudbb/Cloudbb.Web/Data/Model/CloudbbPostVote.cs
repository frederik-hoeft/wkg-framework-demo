using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

public sealed class CloudbbPostVote() : CloudbbVote, IDiscoverableModelConfiguration<CloudbbPostVote>
{
    public Guid PostId { get; set; }

    public CloudbbPost Post { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<CloudbbPostVote> self)
    {
        ArgumentNullException.ThrowIfNull(self);

        self.ToTable("post_votes", table => table.HasCheckConstraint("ck_post_votes_value", "\"value\" in (-1, 1)"))
            .HasKey(my => new { my.PostId, my.UserId });

        self.Property(self => self.PostId)
            .HasColumnName("post_id")
            .HasColumnType("uuid")
            .IsRequired();

        self.HasOne(my => my.Post)
            .WithMany(my => my.Votes)
            .HasForeignKey(my => my.PostId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_post_votes_post_id_posts_id");
    }
}