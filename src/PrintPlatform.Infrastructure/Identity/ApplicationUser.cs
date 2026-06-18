using Microsoft.AspNetCore.Identity;
using PrintPlatform.Domain.Identity;

namespace PrintPlatform.Infrastructure.Identity;

/// <summary>
/// ASP.NET Core Identity user — owns credentials (password hash, lockout, security
/// stamp) only. Domain attributes live on <see cref="User"/> and are linked 1:1 by id.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Role captured at registration; mirrors the domain <see cref="UserRole"/>.</summary>
    public UserRole Role { get; set; }
}

/// <summary>ASP.NET Identity role with Guid keys.</summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string name) : base(name) { }
}
