using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Domain.Marketplace;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Marketplace;

// ===========================================================================
// AddPrinter
// ===========================================================================

public sealed record MaterialAttachment(Guid MaterialOptionId, PrintQuality MaxQuality, bool IsDefault);

public sealed record AddPrinterCommand(
    Guid PrinterOwnerProfileId,
    string Brand,
    string Model,
    TechnologyType TechnologyType,
    int BuildVolumeX,
    int BuildVolumeY,
    int BuildVolumeZ,
    decimal? NozzleDiameter,
    string? SupportedMaterialsJson,
    decimal MaxLayerHeight,
    decimal MinLayerHeight,
    string AreaDistrict,
    string AreaCity,
    bool IsDeliverable,
    IReadOnlyList<MaterialAttachment>? Materials = null)
    : IRequest<Result<PrinterDto>>;

public sealed class AddPrinterCommandValidator : AbstractValidator<AddPrinterCommand>
{
    public AddPrinterCommandValidator()
    {
        RuleFor(x => x.PrinterOwnerProfileId).NotEmpty();
        RuleFor(x => x.Brand).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BuildVolumeX).GreaterThan(0);
        RuleFor(x => x.BuildVolumeY).GreaterThan(0);
        RuleFor(x => x.BuildVolumeZ).GreaterThan(0);
        RuleFor(x => x.MinLayerHeight).GreaterThan(0);
        RuleFor(x => x.MaxLayerHeight).GreaterThanOrEqualTo(x => x.MinLayerHeight);
        RuleFor(x => x.AreaDistrict).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AreaCity).NotEmpty().MaximumLength(100);
    }
}

public sealed class AddPrinterCommandHandler(IMarketplaceDbContext db)
    : IRequestHandler<AddPrinterCommand, Result<PrinterDto>>
{
    public async Task<Result<PrinterDto>> Handle(AddPrinterCommand request, CancellationToken ct)
    {
        var result = Printer.Register(
            request.PrinterOwnerProfileId,
            request.Brand,
            request.Model,
            request.TechnologyType,
            request.BuildVolumeX,
            request.BuildVolumeY,
            request.BuildVolumeZ,
            request.NozzleDiameter,
            request.SupportedMaterialsJson,
            request.MaxLayerHeight,
            request.MinLayerHeight,
            request.AreaDistrict,
            request.AreaCity,
            request.IsDeliverable);

        if (result.IsFailure)
            return Result.Failure<PrinterDto>(result.Error);

        var printer = result.Value;

        foreach (var m in request.Materials ?? [])
        {
            var attach = printer.AddMaterial(m.MaterialOptionId, m.MaxQuality, m.IsDefault);
            if (attach.IsFailure)
                return Result.Failure<PrinterDto>(attach.Error);
        }

        db.Printers.Add(printer);
        await db.SaveChangesAsync(ct);

        return MarketplaceMappings.ToDto(printer);
    }
}

// ===========================================================================
// UpdatePrinterAvailability
// ===========================================================================

public sealed record UpdatePrinterAvailabilityCommand(
    Guid PrinterId,
    Guid PrinterOwnerProfileId,
    PrinterStatus Status,
    int? MaxConcurrentJobs = null)
    : IRequest<Result<PrinterDto>>;

public sealed class UpdatePrinterAvailabilityCommandHandler(IMarketplaceDbContext db)
    : IRequestHandler<UpdatePrinterAvailabilityCommand, Result<PrinterDto>>
{
    public async Task<Result<PrinterDto>> Handle(UpdatePrinterAvailabilityCommand request, CancellationToken ct)
    {
        var printer = await db.Printers
            .FirstOrDefaultAsync(p => p.Id == request.PrinterId, ct);

        if (printer is null)
            return Result.Failure<PrinterDto>(Error.NotFound("Printer.NotFound", "Printer not found."));

        if (printer.PrinterOwnerProfileId != request.PrinterOwnerProfileId)
            return Result.Failure<PrinterDto>(
                Error.Unauthorized("Printer.Forbidden", "You do not own this printer."));

        var update = printer.UpdateAvailability(request.Status, request.MaxConcurrentJobs);
        if (update.IsFailure)
            return Result.Failure<PrinterDto>(update.Error);

        await db.SaveChangesAsync(ct);
        return MarketplaceMappings.ToDto(printer);
    }
}

// ===========================================================================
// VerifyPrinter
// ===========================================================================

public sealed record VerifyPrinterCommand(Guid PrinterId, int CertificationLevel)
    : IRequest<Result<PrinterDto>>;

public sealed class VerifyPrinterCommandValidator : AbstractValidator<VerifyPrinterCommand>
{
    public VerifyPrinterCommandValidator()
        => RuleFor(x => x.CertificationLevel).InclusiveBetween(0, 5);
}

public sealed class VerifyPrinterCommandHandler(IMarketplaceDbContext db)
    : IRequestHandler<VerifyPrinterCommand, Result<PrinterDto>>
{
    public async Task<Result<PrinterDto>> Handle(VerifyPrinterCommand request, CancellationToken ct)
    {
        var printer = await db.Printers.FirstOrDefaultAsync(p => p.Id == request.PrinterId, ct);
        if (printer is null)
            return Result.Failure<PrinterDto>(Error.NotFound("Printer.NotFound", "Printer not found."));

        var verify = printer.Verify(request.CertificationLevel);
        if (verify.IsFailure)
            return Result.Failure<PrinterDto>(verify.Error);

        await db.SaveChangesAsync(ct);
        return MarketplaceMappings.ToDto(printer);
    }
}

// ===========================================================================
// UpdateCertificationScore
// ===========================================================================

public sealed record UpdateCertificationScoreCommand(Guid PrinterId, int CertificationLevel)
    : IRequest<Result<PrinterDto>>;

public sealed class UpdateCertificationScoreCommandValidator : AbstractValidator<UpdateCertificationScoreCommand>
{
    public UpdateCertificationScoreCommandValidator()
        => RuleFor(x => x.CertificationLevel).InclusiveBetween(0, 5);
}

public sealed class UpdateCertificationScoreCommandHandler(IMarketplaceDbContext db)
    : IRequestHandler<UpdateCertificationScoreCommand, Result<PrinterDto>>
{
    public async Task<Result<PrinterDto>> Handle(UpdateCertificationScoreCommand request, CancellationToken ct)
    {
        var printer = await db.Printers.FirstOrDefaultAsync(p => p.Id == request.PrinterId, ct);
        if (printer is null)
            return Result.Failure<PrinterDto>(Error.NotFound("Printer.NotFound", "Printer not found."));

        var update = printer.UpdateCertificationScore(request.CertificationLevel);
        if (update.IsFailure)
            return Result.Failure<PrinterDto>(update.Error);

        await db.SaveChangesAsync(ct);
        return MarketplaceMappings.ToDto(printer);
    }
}
