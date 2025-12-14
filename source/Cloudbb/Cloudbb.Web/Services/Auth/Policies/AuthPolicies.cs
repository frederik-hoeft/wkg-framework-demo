using System.Collections.Immutable;

namespace Cloudbb.Web.Services.Auth.Policies;

internal static class AuthPolicies
{
    public const string USER = "user";
    public const string ADMIN = "admin";

    public static Policy User { get; } = new(USER, [AuthRoles.USER]);

    public static Policy Admin { get; } = new(ADMIN, [AuthRoles.ADMIN]);

    public sealed record Policy(string Name, ImmutableArray<string> Roles);
}