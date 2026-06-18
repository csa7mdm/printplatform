using FluentValidation;
using MediatR;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Application.Identity.DTOs;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Commands;

public sealed record RegisterCustomerCommand(
    string Email,
    string PhoneNumber,
    string Password,
    string FullNameAr,
    string FullNameEn,
    Lang Lang) : IRequest<Result<RegistrationResultDto>>;

public sealed class RegisterCustomerValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?[0-9]{8,15}$")
            .WithMessage("Phone number must be 8-15 digits, optionally prefixed with '+'.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.FullNameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FullNameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Lang).IsInEnum();
    }
}

public sealed class RegisterCustomerHandler
    : IRequestHandler<RegisterCustomerCommand, Result<RegistrationResultDto>>
{
    private readonly IIdentityRepository _repo;
    private readonly IUserCredentialStore _credentials;
    private readonly IOtpSender _otp;

    public RegisterCustomerHandler(
        IIdentityRepository repo, IUserCredentialStore credentials, IOtpSender otp)
    {
        _repo = repo;
        _credentials = credentials;
        _otp = otp;
    }

    public async Task<Result<RegistrationResultDto>> Handle(
        RegisterCustomerCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();
        var phone = cmd.PhoneNumber.Trim();

        if (await _credentials.EmailExistsAsync(email, ct))
            return Result.Failure<RegistrationResultDto>(IdentityErrors.EmailAlreadyInUse);
        if (await _credentials.PhoneExistsAsync(phone, ct))
            return Result.Failure<RegistrationResultDto>(IdentityErrors.PhoneAlreadyInUse);

        var created = await _credentials.CreateAsync(email, phone, cmd.Password, UserRole.Customer, ct);
        if (created.IsFailure)
            return Result.Failure<RegistrationResultDto>(created.Error);

        var userId = created.Value;
        var user = User.Register(userId, email, phone, cmd.FullNameAr, cmd.FullNameEn, UserRole.Customer, cmd.Lang);
        await _repo.AddUserAsync(user, ct);

        var profile = CustomerProfile.Create(userId);
        // Locally-allocated loyalty membership id (kept decoupled from the Loyalty package surface).
        profile.LinkLoyaltyMember($"LOYAL-{userId:N}");
        await _repo.AddCustomerProfileAsync(profile, ct);

        await _repo.SaveChangesAsync(ct);

        await _otp.SendAsync(phone, cmd.Lang, ct);

        return Result.Success(new RegistrationResultDto(
            userId, email, phone, UserRole.Customer, OtpSent: true));
    }
}
