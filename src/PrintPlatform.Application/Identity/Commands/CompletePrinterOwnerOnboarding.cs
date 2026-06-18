using FluentValidation;
using MediatR;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Application.Identity.DTOs;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Commands;

public sealed record CompletePrinterOwnerOnboardingCommand(
    Guid UserId,
    string BankAccountDetails,
    string? InstapayNumber) : IRequest<Result<PrinterOwnerProfileDto>>;

public sealed class CompletePrinterOwnerOnboardingValidator
    : AbstractValidator<CompletePrinterOwnerOnboardingCommand>
{
    public CompletePrinterOwnerOnboardingValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BankAccountDetails).NotEmpty().MaximumLength(512);
        RuleFor(x => x.InstapayNumber).Matches(@"^\+?[0-9]{8,15}$")
            .When(x => !string.IsNullOrWhiteSpace(x.InstapayNumber));
    }
}

public sealed class CompletePrinterOwnerOnboardingHandler
    : IRequestHandler<CompletePrinterOwnerOnboardingCommand, Result<PrinterOwnerProfileDto>>
{
    private readonly IIdentityRepository _repo;
    private readonly IFieldEncryptor _encryptor;

    public CompletePrinterOwnerOnboardingHandler(IIdentityRepository repo, IFieldEncryptor encryptor)
    {
        _repo = repo;
        _encryptor = encryptor;
    }

    public async Task<Result<PrinterOwnerProfileDto>> Handle(
        CompletePrinterOwnerOnboardingCommand cmd, CancellationToken ct)
    {
        var profile = await _repo.GetPrinterOwnerProfileByUserIdAsync(cmd.UserId, ct);
        if (profile is null)
            return Result.Failure<PrinterOwnerProfileDto>(IdentityErrors.ProfileNotFound);

        var encryptedBank = _encryptor.Encrypt(cmd.BankAccountDetails.Trim());
        var result = profile.CompleteOnboarding(encryptedBank, cmd.InstapayNumber?.Trim());
        if (result.IsFailure)
            return Result.Failure<PrinterOwnerProfileDto>(result.Error);

        await _repo.SaveChangesAsync(ct);

        return Result.Success(MapToDto(profile));
    }

    internal static PrinterOwnerProfileDto MapToDto(PrinterOwnerProfile p)
        => new(
            p.UserId,
            p.BusinessName,
            NationalIdMasked: "***************",
            p.InstapayNumber,
            p.CertificationLevel,
            p.CertificationScore,
            p.GamificationPlayerId,
            p.IsAvailable,
            p.OnboardingCompleted,
            p.OnboardingCompletedAt);
}
