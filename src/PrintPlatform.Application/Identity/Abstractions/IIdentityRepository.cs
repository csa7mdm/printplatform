using PrintPlatform.Domain.Identity;

namespace PrintPlatform.Application.Identity.Abstractions;

/// <summary>Persistence gateway for the Identity aggregate(s). Implemented in Infrastructure.</summary>
public interface IIdentityRepository
{
    // -- Users -----------------------------------------------------------
    Task AddUserAsync(User user, CancellationToken ct);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct);

    /// <summary>Loads a user by an active refresh-token hash (includes the tokens collection).</summary>
    Task<User?> GetUserByRefreshTokenHashAsync(string tokenHash, CancellationToken ct);

    // -- Profiles --------------------------------------------------------
    Task AddCustomerProfileAsync(CustomerProfile profile, CancellationToken ct);
    Task AddPrinterOwnerProfileAsync(PrinterOwnerProfile profile, CancellationToken ct);

    Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(Guid userId, CancellationToken ct);
    Task<PrinterOwnerProfile?> GetPrinterOwnerProfileByUserIdAsync(Guid userId, CancellationToken ct);

    /// <summary>Commits all tracked changes; dispatches domain events after commit.</summary>
    Task SaveChangesAsync(CancellationToken ct);
}
