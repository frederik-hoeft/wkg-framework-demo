using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

public sealed class CloudbbPostRevision() : CloudbbEntity, IReflectiveModelConfiguration<CloudbbPostRevision>
{
    [SetsRequiredMembers]
    public CloudbbPostRevision(string title, string content) : this()
    {
        Title = title;
        Content = content;
    }

    public Guid PostId { get; set; }

    public required string Title { get; set; }

    public required string Content { get; set; }

    public DateTime CreationTime { get; set; }

    public CloudbbPost Post { get; set; } = null!;

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