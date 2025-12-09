using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Represents a user vote on a comment (upvote or downvote).
/// Used for indicating comment quality and relevance to the discussion.
/// Each user can have at most one vote per comment.
/// </summary>
public sealed class CloudbbCommentVote() : CloudbbVote, IDiscoverableModelConfiguration<CloudbbCommentVote>
{
    /// <summary>
    /// Foreign key referencing the comment being voted on.
    /// </summary>
    public Guid CommentId { get; set; }

    /// <summary>
    /// Navigation property to the comment being voted on.
    /// </summary>
    public CloudbbComment Comment { get; set; } = null!;

    /// <inheritdoc />
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