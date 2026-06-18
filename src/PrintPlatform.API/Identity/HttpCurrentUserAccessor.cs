using System.Security.Claims;
using PrintPlatform.Application.Identity.Abstractions;

namespace PrintPlatform.API.Identity;

/// <summary>
/// Resolves the current user id and originating IP from the active HTTP request.
/// Registered in the API layer because it depends on <see cref="IHttpContextAccessor"/>.
/// </summary>
public sealed class HttpCurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUserAccessor(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid? UserId
    {
        get
        {
            var raw = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? _accessor.HttpContext?.User.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? IpAddress
        => _accessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
}
