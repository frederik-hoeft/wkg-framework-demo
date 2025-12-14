using Cloudbb.Web.Api.V1.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Represents a forum post in the Cloudbb system.
/// Posts are the primary content containers that users create to start discussions.
/// Supports versioning through revisions and user interactions through voting and comments.
/// </summary>
public sealed partial class CloudbbPost() : CloudbbEntity, IDiscoverableModelConfiguration<CloudbbPost>
{
    /// <summary>
    /// Foreign key referencing the user who authored this post.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user who authored this post.
    /// Provides access to author information for display and permissions.
    /// </summary>
    public CloudbbUser User { get; set; } = null!;

    /// <summary>
    /// Collection of all revisions for this post, tracking edit history.
    /// The latest revision contains the current title and content.
    /// </summary>
    public List<CloudbbPostRevision> Revisions { get; set; } = null!;

    /// <summary>
    /// Collection of user comments on this post.
    /// Enables threaded discussions and user engagement.
    /// </summary>
    public List<CloudbbComment> Comments { get; set; } = null!;

    /// <summary>
    /// Collection of user votes (upvotes/downvotes) for this post.
    /// Used for calculating post score and ranking in listings.
    /// </summary>
    public List<CloudbbPostVote> Votes { get; set; } = null!;

    /// <inheritdoc />
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