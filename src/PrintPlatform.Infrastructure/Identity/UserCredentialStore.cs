using Microsoft.AspNetCore.Identity;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Infrastructure.Identity;

/// <summary>Bridges to ASP.NET Core Identity for credential creation/validation.</summary>
public sealed class UserCredentialStore : IUserCredentialStore
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserCredentialStore(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct)
        => await _userManager.FindByEmailAsync(email) is not null;

    public Task<bool> PhoneExistsAsync(string phoneNumber, CancellationToken ct)
        => Task.FromResult(_userManager.Users.Any(u => u.PhoneNumber == phoneNumber));

    public async Task<Result<Guid>> CreateAsync(
        string email, string phoneNumber, string password, UserRole role, CancellationToken ct)
    {
        var appUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            PhoneNumber = phoneNumber,
            Role = role,
        };

        var result = await _userManager.CreateAsync(appUser, password);
        if (!result.Succeeded)
        {
            var first = result.Errors.FirstOrDefault();
            return Result.Failure<Guid>(Error.Validation(
                $"Identity.Credentials.{first?.Code ?? "CreateFailed"}",
                first?.Description ?? "Failed to create credentials."));
        }

        await _userManager.AddToRoleAsync(appUser, role.ToString());
        return Result.Success(appUser.Id);
    }

    public async Task<Result<Guid>> ValidateCredentialsAsync(
        string email, string password, CancellationToken ct)
    {
        var appUser = await _userManager.FindByEmailAsync(email);
        if (appUser is null)
            return Result.Failure<Guid>(IdentityErrors.InvalidCredentials);

        if (await _userManager.IsLockedOutAsync(appUser))
            return Result.Failure<Guid>(IdentityErrors.AccountInactive);

        if (!await _userManager.CheckPasswordAsync(appUser, password))
        {
            await _userManager.AccessFailedAsync(appUser);
            return Result.Failure<Guid>(IdentityErrors.InvalidCredentials);
        }

        await _userManager.ResetAccessFailedCountAsync(appUser);
        return Result.Success(appUser.Id);
    }
}
