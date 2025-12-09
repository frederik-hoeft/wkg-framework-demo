using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Represents a specific revision of a forum post, enabling version history tracking.
/// Each time a post is edited, a new revision is created to maintain content history
/// and support features like edit tracking and content rollback.
/// </summary>
public sealed class CloudbbPostRevision() : CloudbbEntity, IDiscoverableModelConfiguration<CloudbbPostRevision>
{
    /// <summary>
    /// Initializes a new post revision with the specified title and content.
    /// </summary>
    /// <param name="title">The post title for this revision.</param>
    /// <param name="content">The post content for this revision.</param>
    [SetsRequiredMembers]
    public CloudbbPostRevision(string title, string content) : this()
    {
        Title = title;
        Content = content;
    }

    /// <summary>
    /// Foreign key referencing the post this revision belongs to.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// The post title as it appeared in this revision.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The post content as it appeared in this revision.
    /// Supports rich text or markdown formatting.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Timestamp when this revision was created.
    /// Used for chronological ordering and edit history display.
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Navigation property to the post this revision belongs to.
    /// </summary>
    public CloudbbPost Post { get; set; } = null!;

    /// <inheritdoc />
    public static void Configure(EntityTypeBuilder<CloudbbPostRevision> self)
    {
        ArgumentNullException.ThrowIfNull(self);

        self.ToTable("post_revisions").HasKey(my => my.Id);

        self.Property(my => my.PostId)
            .HasColumnName("post_id")
            .HasColumnType("uuid")
            .IsRequired();

        self.Property(my => my.Title)
            .HasColumnName("title")
            .HasColumnType("varchar")
            .HasMaxLength(256)
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

        self.HasOne(my => my.Post)
            .WithMany(my => my.Revisions)
            .HasForeignKey(my => my.PostId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired()
            .HasConstraintName("fk_post_revisions_posts_post_id");

        self.HasIndex(my => my.Title)
            .HasDatabaseName("ix_post_revisions_title");
    }
}