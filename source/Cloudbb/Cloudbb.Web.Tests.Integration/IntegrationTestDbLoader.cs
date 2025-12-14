using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth.Policies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using Wkg.AspNetCore.TestAdapters.Initialization;

namespace Cloudbb.Web.Tests.Integration;

public sealed class IntegrationTestDbLoader : AsyncTestDatabaseLoader<IntegrationTestDbLoader, CloudbbDbContext>, IAsyncTestDatabaseLoader<CloudbbDbContext>
{
    internal static TestUser TestUser1 { get; } = TestUser.Create("GlobalTestUser1", "global-test-user1@example.com", "P@ssw0rdGlobalTestUser1", AuthPolicies.User.Roles);

    internal static TestUser TestUser2 { get; } = TestUser.Create("GlobalTestUser2", "global-test-user2@example.com", "P@ssw0rdGlobalTestUser2", AuthPolicies.User.Roles);

    public static Guid Post1OfUser1Id { get; } = Guid.CreateVersion7();

    public static Guid Post2OfUser1Id { get; } = Guid.CreateVersion7();

    public static Guid PostOfUser2Id { get; } = Guid.CreateVersion7();

    public static Guid Comment1OnPost1Id { get; } = Guid.CreateVersion7();

    public static Guid Comment2OnPost1Id { get; } = Guid.CreateVersion7();

    public async ValueTask InitializeDatabaseAsync(CloudbbDbContext dbContext, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        // Create test users
        UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        
        // Primary test user
        IdentityUser identityUser = new()
        {
            UserName = TestUser1.Username,
            Email = TestUser1.Email
        };
        IdentityResult createResult = await userManager.CreateAsync(identityUser, TestUser1.Password);
        Assert.IsTrue(createResult.Succeeded);
        foreach (string role in TestUser1.Roles)
        {
            await userManager.AddToRoleAsync(identityUser, role);
        }
        CloudbbUser user = new(identityUser)
        {
            Id = TestUser1.UserId
        };
        dbContext.Add(user);
        
        // Second test user for voting/interaction tests
        IdentityUser identityUser2 = new()
        {
            UserName = TestUser2.Username,
            Email = TestUser2.Email
        };
        IdentityResult createResult2 = await userManager.CreateAsync(identityUser2, TestUser2.Password);
        Assert.IsTrue(createResult2.Succeeded);

        foreach (string role in TestUser2.Roles)
        {
            await userManager.AddToRoleAsync(identityUser2, role);
        }
        CloudbbUser user2 = new(identityUser2)
        {
            Id = TestUser2.UserId
        };
        dbContext.Add(user2);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Create test posts with revisions
        DateTime now = DateTime.UtcNow;
        
        // Post 1: Popular post with high score
        CloudbbPost post1 = new()
        {
            Id = Post1OfUser1Id,
            UserId = user.Id,
            Revisions = 
            [
                new CloudbbPostRevision("Popular Post Title", "This is a popular post with lots of upvotes and content that is engaging to users.")
            ]
        };
        dbContext.Add(post1);
        
        // Post 2: Recent post with moderate score  
        CloudbbPost post2 = new()
        {
            Id = PostOfUser2Id,
            UserId = user2.Id,
            Revisions = 
            [
                new CloudbbPostRevision("Recent Post", "This is a different user's post without much interaction yet.")
            ]
        };
        dbContext.Add(post2);
        
        // Post 3: Post by primary user that can be edited/deleted
        CloudbbPost post3 = new()
        {
            Id = Post2OfUser1Id,
            UserId = user.Id,
            Revisions = 
            [
                new CloudbbPostRevision
                (
                    "Editable Post",
                    """
                    Lorizzle ipsum dolizzle stuff fizzle, consectetuer adipiscing break it down. Nullizzle sapien velizzle, my shizz pimpin', shizzle my nizzle crocodizzle shut the shizzle up, gravida vizzle, dang.
                
                    Pellentesque we gonna chung tortor. Sizzle pizzle. Fizzle izzle dolor dapibus fo shizzle mah nizzle fo rizzle, mah home g-dizzle tempus tempor. Maurizzle cool nibh owned turpizzle. 
                    My shizz fo shizzle tortor.
                    """
                )
            ]
        };
        dbContext.Add(post3);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Add votes to simulate community interaction
        // Post 1: High score (+3)
        dbContext.Add(new CloudbbPostVote { PostId = post1.Id, UserId = user2.Id, Value = 1 }); // upvote from user2

        // Add comments to posts
        CloudbbComment comment1 = new()
        {
            Id = Comment1OnPost1Id,
            PostId = post1.Id,
            UserId = user2.Id,
            Content = "Great post! Thanks for sharing.",
            CreationTime = now.AddDays(-4)
        };
        dbContext.Add(comment1);
        
        CloudbbComment comment2 = new()
        {
            Id = Comment2OnPost1Id,
            PostId = post1.Id, 
            UserId = user.Id,
            Content = "Thanks for the feedback!",
            CreationTime = now.AddDays(-3)
        };
        dbContext.Add(comment2);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

internal sealed record TestUser(Guid UserId, string Username, string Email, string Password, ImmutableArray<string> Roles)
{
    public static TestUser Create(string Username, string Email, string Password, params ImmutableArray<string> roles) =>
        new(Guid.CreateVersion7(), Username, Email, Password, roles);
}