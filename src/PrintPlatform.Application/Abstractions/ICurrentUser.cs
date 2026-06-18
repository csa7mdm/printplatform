namespace PrintPlatform.Application.Abstractions;

/// <summary>Ambient information about the authenticated caller (resolved from the JWT).</summary>
public interface ICurrentUser
{
    /// <summary>The authenticated user's id, or null for anonymous requests.</summary>
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
}
