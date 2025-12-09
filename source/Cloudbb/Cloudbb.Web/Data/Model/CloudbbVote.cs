using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Base class for user voting entities in the forum system.
/// Provides common voting functionality for posts and comments,
/// implementing a simple upvote/downvote system with value constraints.
/// </summary>
public abstract class CloudbbVote : ICloudbbConnectionEntity, IDiscoverableBaseModelConfiguration<CloudbbVote>
{
    /// <summary>
    /// Foreign key referencing the user who cast this vote.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The vote value: 1 for upvote, -1 for downvote.
    /// Constrained by database check constraints to ensure valid values.
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Navigation property to the user who cast this vote.
    /// </summary>
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