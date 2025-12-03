using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

public sealed class CloudbbCommentVote() : CloudbbVote, IReflectiveModelConfiguration<CloudbbCommentVote>
{
    public Guid CommentId { get; set; }

    public CloudbbComment Comment { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<CloudbbCommentVote> self)
    {
        ArgumentNullException.ThrowIfNull(self);

        self.ToTable("comment_votes", table => table.HasCheckConstraint("ck_comment_votes_value", "\"value\" in (-1, 1)"))
            .HasKey(my => new { my.CommentId, my.UserId });

        self.Property(self => self.CommentId)
            .HasColumnName("comment_id")
            .HasColumnType("uuid")
            .IsRequired();

        self.HasOne(my => my.Comment)
            .WithMany(my => my.Votes)
            .HasForeignKey(my => my.CommentId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_comment_votes_comment_id_comments_id");
    }
}