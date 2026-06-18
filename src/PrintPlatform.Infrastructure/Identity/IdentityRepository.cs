using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Infrastructure.Data;

namespace PrintPlatform.Infrastructure.Identity;

/// <summary>EF Core implementation of <see cref="IIdentityRepository"/>.</summary>
public sealed class IdentityRepository : IIdentityRepository
{
    private readonly AppDbContext _db;

    public IdentityRepository(AppDbContext db) => _db = db;

    public async Task AddUserAsync(User user, CancellationToken ct)
        => await _db.Users.AddAsync(user, ct);

    public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct)
        => _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _db.Users.FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public Task<User?> GetUserByRefreshTokenHashAsync(string tokenHash, CancellationToken ct)
        => _db.Users
            .Where(u => u.RefreshTokens.Any(t => t.TokenHash == tokenHash))
            .FirstOrDefaultAsync(ct);

    public async Task AddCustomerProfileAsync(CustomerProfile profile, CancellationToken ct)
        => await _db.CustomerProfiles.AddAsync(profile, ct);

    public async Task AddPrinterOwnerProfileAsync(PrinterOwnerProfile profile, CancellationToken ct)
        => await _db.PrinterOwnerProfiles.AddAsync(profile, ct);

    public Task<CustomerProfile?> GetCustomerProfileByUserIdAsync(Guid userId, CancellationToken ct)
        => _db.CustomerProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public Task<PrinterOwnerProfile?> GetPrinterOwnerProfileByUserIdAsync(Guid userId, CancellationToken ct)
        => _db.PrinterOwnerProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
