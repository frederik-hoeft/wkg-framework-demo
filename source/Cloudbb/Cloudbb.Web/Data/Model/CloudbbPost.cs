using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

public sealed class CloudbbPost() : CloudbbEntity, IReflectiveModelConfiguration<CloudbbPost>
{
    public Guid UserId { get; set; }

    public CloudbbUser User { get; set; } = null!;

    public ICollection<CloudbbPostRevision> Revisions { get; set; } = null!;

    public ICollection<CloudbbComment> Comments { get; set; } = null!;

    public ICollection<CloudbbPostVote> Votes { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<CloudbbPost> self)
    {
        ArgumentNullException.ThrowIfNull(self);

        self.ToTable("posts").HasKey(my => my.Id);

        self.Property(my => my.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        self.HasOne(my => my.User)
            .WithMany(my => my.Posts)
            .HasForeignKey(my => my.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_posts_user_id");
    }
}