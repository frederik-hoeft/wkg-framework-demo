using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using Wkg.Data.Validation;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

public sealed class CloudbbUser() : CloudbbEntity, IDiscoverableModelConfiguration<CloudbbUser>
{
    [SetsRequiredMembers]
    public CloudbbUser(IdentityUser identityUser) : this()
    {
        ArgumentNullException.ThrowIfNull(identityUser);
        string email = identityUser.Email ?? throw new ArgumentException("Identity user must have an email.", nameof(identityUser));
        string userName = identityUser.UserName ?? throw new ArgumentException("Identity user must have a username.", nameof(identityUser));
        if (!DataValidationService.IsEmailAddress(email))
        {
            throw new ArgumentException("Identity user must have a valid email.", nameof(identityUser));
        }

        IdentityUser = identityUser;
        IdentityUserId = identityUser.Id;
        Email = email;
        UserName = userName;
    }

    public required string Email { get; set; }

    public required string UserName { get; set; }

    public required string IdentityUserId { get; set; }

    public IdentityUser IdentityUser { get; set; } = null!;

    public ICollection<CloudbbPost> Posts { get; set; } = null!;

    public ICollection<CloudbbComment> Comments { get; set; } = null!;

    public static void Configure(EntityTypeBuilder<CloudbbUser> self)
    {
        ArgumentNullException.ThrowIfNull(self);

        self.ToTable("users").HasKey(my => my.Id);

        self.Property(my => my.Email)
            .HasColumnName("email")
            .HasColumnType("varchar")
            .HasMaxLength(254)
            .IsRequired();

        self.Property(my => my.UserName)
            .HasColumnName("username")
            .HasColumnType("varchar")
            .HasMaxLength(32)
            .IsRequired();

        self.Property(my => my.IdentityUserId)
            .HasColumnName("identity_user_id")
            // data type from generated ASP.NET Identity schema
            .HasColumnType("text")
            .IsRequired();

        self.HasIndex(my => my.Email, "idx_users_email").IsUnique();
        self.HasIndex(my => my.UserName, "idx_users_username").IsUnique();

        self.HasOne(my => my.IdentityUser)
            .WithOne()
            .HasForeignKey<CloudbbUser>(my => my.IdentityUserId)
            .HasConstraintName("fk_users_identity_user_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}