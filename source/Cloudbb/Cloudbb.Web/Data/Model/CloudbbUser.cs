using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics.CodeAnalysis;
using Wkg.Data.Validation;
using Wkg.EntityFrameworkCore.Configuration;

namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Domain entity representing a forum user in the Cloudbb system.
/// Acts as a bridge between ASP.NET Core Identity and the forum's domain model,
/// extending user data with forum-specific properties and relationships.
/// </summary>
public sealed class CloudbbUser() : CloudbbEntity, IDiscoverableModelConfiguration<CloudbbUser>
{
    /// <summary>
    /// Initializes a new CloudbbUser from an existing ASP.NET Core Identity user.
    /// Validates that the identity user has required email and username properties.
    /// </summary>
    /// <param name="identityUser">The Identity user to create the domain user from.</param>
    /// <exception cref="ArgumentNullException">Thrown when identityUser is null.</exception>
    /// <exception cref="ArgumentException">Thrown when identity user lacks email or username, or email is invalid.</exception>
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

    /// <summary>
    /// The user's email address, synchronized with the Identity user's email.
    /// Required for forum notifications and account verification.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// The user's display name in the forum, synchronized with the Identity username.
    /// Used for post attribution and user identification in the forum interface.
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Foreign key linking to the ASP.NET Core Identity user.
    /// Maintains the relationship between forum data and authentication data.
    /// </summary>
    public required string IdentityUserId { get; set; }

    /// <summary>
    /// Navigation property to the associated ASP.NET Core Identity user.
    /// Provides access to authentication and authorization data.
    /// </summary>
    public IdentityUser IdentityUser { get; set; } = null!;

    /// <summary>
    /// Collection of forum posts authored by this user.
    /// Supports lazy loading and tracks user's contribution history.
    /// </summary>
    public ICollection<CloudbbPost> Posts { get; set; } = null!;

    /// <summary>
    /// Collection of comments authored by this user across all forum posts.
    /// Enables tracking of user engagement and comment history.
    /// </summary>
    public ICollection<CloudbbComment> Comments { get; set; } = null!;

    /// <inheritdoc />
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