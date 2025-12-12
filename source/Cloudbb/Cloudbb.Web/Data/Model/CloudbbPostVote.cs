using Cloudbb.Web.Api.V1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Represents a user vote on a forum post (upvote or downvote).
/// Used for calculating post scores and enabling community-driven content ranking.
/// Each user can have at most one vote per post.
/// </summary>
public sealed partial class CloudbbPostVote() : CloudbbVote, IDiscoverableModelConfiguration<CloudbbPostVote>
{
    /// <summary>
    /// Foreign key referencing the post being voted on.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Navigation property to the post being voted on.
    /// </summary>
    public CloudbbPost Post { get; set; } = null!;

    /// <inheritdoc />
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