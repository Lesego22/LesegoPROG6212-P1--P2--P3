using Microsoft.AspNetCore.Http;
using RaceDay.Web.Models;

namespace RaceDay.Web.Services;


public static class UserSession
{
    private const string TokenKey = "token";
    private const string UserIdKey = "userId";
    private const string RoleKey = "role";
    private const string NameKey = "name";

    public static void SignIn(this ISession session, AuthResponse auth, string displayName)
    {
        session.SetString(TokenKey, auth.Token);
        session.SetString(UserIdKey, auth.UserId.ToString());
        session.SetString(RoleKey, auth.Role);
        session.SetString(NameKey, displayName);
    }

    public static void SetDisplayName(this ISession session, string name) => session.SetString(NameKey, name);

    public static string? GetToken(this ISession session) => session.GetString(TokenKey);

    public static string? GetRole(this ISession session) => session.GetString(RoleKey);

    public static string GetDisplayName(this ISession session) => session.GetString(NameKey) ?? string.Empty;

    public static Guid? GetUserId(this ISession session) =>
        Guid.TryParse(session.GetString(UserIdKey), out var id) ? (Guid?)id : null;

    public static bool IsLoggedIn(this ISession session) => !string.IsNullOrEmpty(session.GetString(TokenKey));
}
