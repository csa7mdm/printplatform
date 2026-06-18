using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Domain.Marketplace;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Marketplace;

// ===========================================================================
// GetMyPrinters
// ===========================================================================

public sealed record GetMyPrintersQuery(Guid PrinterOwnerProfileId)
    : IRequest<Result<IReadOnlyList<PrinterDto>>>;

public sealed class GetMyPrintersQueryHandler(IMarketplaceDbContext db)
    : IRequestHandler<GetMyPrintersQuery, Result<IReadOnlyList<PrinterDto>>>
{
    public async Task<Result<IReadOnlyList<PrinterDto>>> Handle(GetMyPrintersQuery request, CancellationToken ct)
    {
        var printers = await db.Printers
            .AsNoTracking()
            .Where(p => p.PrinterOwnerProfileId == request.PrinterOwnerProfileId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);

        IReadOnlyList<PrinterDto> dtos = printers.Select(MarketplaceMappings.ToDto).ToList();
        return Result.Success(dtos);
    }
}

// ===========================================================================
// GetAvailablePrintersForJob (RANKED)
// ===========================================================================

public sealed record GetAvailablePrintersForJobQuery(Guid JobId)
    : IRequest<Result<IReadOnlyList<RankedPrinterDto>>>;

public sealed class GetAvailablePrintersForJobQueryHandler(
    IMarketplaceDbContext db,
    IJobRequirementsProvider jobRequirements,
    IPrinterLoadProvider loadProvider,
    PrinterRankingService ranking)
    : IRequestHandler<GetAvailablePrintersForJobQuery, Result<IReadOnlyList<RankedPrinterDto>>>
{
    public async Task<Result<IReadOnlyList<RankedPrinterDto>>> Handle(
        GetAvailablePrintersForJobQuery request, CancellationToken ct)
    {
        var job = await jobRequirements.GetAsync(request.JobId, ct);
        if (job is null)
            return Result.Failure<IReadOnlyList<RankedPrinterDto>>(
                Error.NotFound("Job.NotFound", "Job not found."));

        // Only active printers are eligible candidates.
        var printers = await db.Printers
            .AsNoTracking()
            .Where(p => p.Status == PrinterStatus.Active)
            .ToListAsync(ct);

        if (printers.Count == 0)
            return Result.Success<IReadOnlyList<RankedPrinterDto>>([]);

        var printerIds = printers.Select(p => p.Id).ToList();

        // Materials supported per printer (join → MaterialOption.MaterialType), active only.
        var materialsByPrinter = await db.PrinterMaterials
            .AsNoTracking()
            .Where(pm => printerIds.Contains(pm.PrinterId)
                      && pm.MaterialOption!.IsActive)
            .Select(pm => new { pm.PrinterId, pm.MaterialOption!.MaterialType })
            .ToListAsync(ct);

        var materialLookup = materialsByPrinter
            .GroupBy(x => x.PrinterId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<MaterialType>)g.Select(x => x.MaterialType).Distinct().ToList());

        var activeJobCounts = await loadProvider.GetActiveJobCountsAsync(printerIds, ct);

        var ranked = new List<RankedPrinterDto>(printers.Count);
        foreach (var printer in printers)
        {
            var mats = materialLookup.TryGetValue(printer.Id, out var m)
                ? m
                : (IReadOnlyCollection<MaterialType>)[];

            var activeJobs = activeJobCounts.TryGetValue(printer.Id, out var c) ? c : 0;

            var scored = ranking.Score(printer, mats, job, activeJobs);
            if (scored is not null)
                ranked.Add(scored);
        }

        IReadOnlyList<RankedPrinterDto> result = ranked
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.DistanceKm)
            .ToList();

        return Result.Success(result);
    }
}

// ===========================================================================
// Materials queries
// ===========================================================================

public sealed record GetMaterialsQuery(bool ActiveOnly = true)
    : IRequest<Result<IReadOnlyList<MaterialOptionDto>>>;

public sealed class GetMaterialsQueryHandler(IMarketplaceDbContext db)
    : IRequestHandler<GetMaterialsQuery, Result<IReadOnlyList<MaterialOptionDto>>>
{
    public async Task<Result<IReadOnlyList<MaterialOptionDto>>> Handle(GetMaterialsQuery request, CancellationToken ct)
    {
        var query = db.MaterialOptions.AsNoTracking();
        if (request.ActiveOnly)
            query = query.Where(m => m.IsActive);

        var materials = await query
            .OrderBy(m => m.MaterialType)
            .ThenBy(m => m.ColorName)
            .ToListAsync(ct);

        IReadOnlyList<MaterialOptionDto> dtos = materials.Select(MarketplaceMappings.ToDto).ToList();
        return Result.Success(dtos);
    }
}

public sealed record GetMaterialByIdQuery(Guid MaterialId)
    : IRequest<Result<MaterialOptionDto>>;

public sealed class GetMaterialByIdQueryHandler(IMarketplaceDbContext db)
    : IRequestHandler<GetMaterialByIdQuery, Result<MaterialOptionDto>>
{
    public async Task<Result<MaterialOptionDto>> Handle(GetMaterialByIdQuery request, CancellationToken ct)
    {
        var material = await db.MaterialOptions
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MaterialId, ct);

        return material is null
            ? Result.Failure<MaterialOptionDto>(Error.NotFound("Material.NotFound", "Material not found."))
            : Result.Success(MarketplaceMappings.ToDto(material));
    }
}
