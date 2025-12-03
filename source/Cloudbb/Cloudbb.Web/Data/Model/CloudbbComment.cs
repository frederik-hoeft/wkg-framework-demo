using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

public sealed class CloudbbComment() : CloudbbEntity, IReflectiveModelConfiguration<CloudbbComment>
{
    public Guid UserId { get; set; }

    public Guid PostId { get; set; }

    public required string Content { get; set; }

    public DateTime CreationTime { get; set; }

    public CloudbbUser User { get; set; } = null!;

    public CloudbbPost Post { get; set; } = null!;

    public ICollection<CloudbbCommentVote> Votes { get; set; } = null!;

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