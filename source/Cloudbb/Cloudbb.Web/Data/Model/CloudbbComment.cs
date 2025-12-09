using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Represents a user comment on a forum post.
/// Comments enable threaded discussions and user engagement with post content.
/// Supports voting to indicate comment quality and relevance.
/// </summary>
public sealed class CloudbbComment() : CloudbbEntity, IDiscoverableModelConfiguration<CloudbbComment>
{
    /// <summary>
    /// Foreign key referencing the user who authored this comment.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Foreign key referencing the post this comment belongs to.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// The text content of the comment. Supports rich text or markdown formatting.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Timestamp when the comment was originally created.
    /// Used for chronological ordering and display.
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Navigation property to the user who authored this comment.
    /// Provides access to author information for display and permissions.
    /// </summary>
    public CloudbbUser User { get; set; } = null!;

    /// <summary>
    /// Navigation property to the post this comment belongs to.
    /// Enables navigation back to the discussion context.
    /// </summary>
    public CloudbbPost Post { get; set; } = null!;

    /// <summary>
    /// Collection of user votes (upvotes/downvotes) for this comment.
    /// Used for calculating comment score and quality ranking.
    /// </summary>
    public ICollection<CloudbbCommentVote> Votes { get; set; } = null!;

    /// <inheritdoc />
    public static void Configure(EntityTypeBuilder<CloudbbComment> self)
    {
        ArgumentNullException.ThrowIfNull(self);

        self.ToTable("comments").HasKey(my => my.Id);

        self.Property(my => my.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        self.Property(my => my.PostId)
            .HasColumnName("post_id")
            .HasColumnType("uuid")
            .IsRequired();

        self.Property(my => my.Content)
            .HasColumnName("content")
            .HasColumnType("text")
            .IsRequired();

        self.Property(my => my.CreationTime)
            .HasColumnName("creation_time")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now() at time zone 'utc'")
            .ValueGeneratedOnAdd()
            .IsRequired();

        self.HasOne(my => my.User)
            .WithMany(my => my.Comments)
            .HasForeignKey(my => my.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName("fk_comments_user_id");

        self.HasOne(my => my.Post)
            .WithMany(my => my.Comments)
            .HasForeignKey(my => my.PostId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName("fk_comments_post_id");
    }
}