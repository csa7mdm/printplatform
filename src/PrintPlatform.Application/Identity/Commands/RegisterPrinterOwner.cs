using FluentValidation;
using MediatR;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Application.Identity.DTOs;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Commands;

public sealed record RegisterPrinterOwnerCommand(
    string Email,
    string PhoneNumber,
    string Password,
    string FullNameAr,
    string FullNameEn,
    string NationalId,
    string BusinessName,
    Lang Lang) : IRequest<Result<RegistrationResultDto>>;

public sealed class RegisterPrinterOwnerValidator : AbstractValidator<RegisterPrinterOwnerCommand>
{
    public RegisterPrinterOwnerValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?[0-9]{8,15}$");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.FullNameAr).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FullNameEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.NationalId).NotEmpty().Matches(@"^[0-9]{14}$")
            .WithMessage("Egyptian national id must be exactly 14 digits.");
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Lang).IsInEnum();
    }
}

public sealed class RegisterPrinterOwnerHandler
    : IRequestHandler<RegisterPrinterOwnerCommand, Result<RegistrationResultDto>>
{
    private readonly IIdentityRepository _repo;
    private readonly IUserCredentialStore _credentials;
    private readonly IFieldEncryptor _encryptor;
    private readonly IOtpSender _otp;

    public RegisterPrinterOwnerHandler(
        IIdentityRepository repo,
        IUserCredentialStore credentials,
        IFieldEncryptor encryptor,
        IOtpSender otp)
    {
        _repo = repo;
        _credentials = credentials;
        _encryptor = encryptor;
        _otp = otp;
    }

    public async Task<Result<RegistrationResultDto>> Handle(
        RegisterPrinterOwnerCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();
        var phone = cmd.PhoneNumber.Trim();

        if (await _credentials.EmailExistsAsync(email, ct))
            return Result.Failure<RegistrationResultDto>(IdentityErrors.EmailAlreadyInUse);
        if (await _credentials.PhoneExistsAsync(phone, ct))
            return Result.Failure<RegistrationResultDto>(IdentityErrors.PhoneAlreadyInUse);

        var created = await _credentials.CreateAsync(email, phone, cmd.Password, UserRole.PrinterOwner, ct);
        if (created.IsFailure)
            return Result.Failure<RegistrationResultDto>(created.Error);

        var userId = created.Value;
        var user = User.Register(userId, email, phone, cmd.FullNameAr, cmd.FullNameEn, UserRole.PrinterOwner, cmd.Lang);
        await _repo.AddUserAsync(user, ct);

        // National id is encrypted before it ever reaches persistence.
        var encryptedNationalId = _encryptor.Encrypt(cmd.NationalId.Trim());
        var profile = PrinterOwnerProfile.Create(userId, encryptedNationalId, cmd.BusinessName);
        profile.LinkGamificationPlayer($"PLAYER-{userId:N}");
        await _repo.AddPrinterOwnerProfileAsync(profile, ct);

        await _repo.SaveChangesAsync(ct);

        await _otp.SendAsync(phone, cmd.Lang, ct);

        return Result.Success(new RegistrationResultDto(
            userId, email, phone, UserRole.PrinterOwner, OtpSent: true));
    }
}
