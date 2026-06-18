using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Marketplace;
using PrintPlatform.Domain.Marketplace;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/printers")]
[Produces("application/json")]
public sealed class PrintersController : ApiControllerBase
{
    /// <summary>Lists printers owned by the authenticated profile.</summary>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(IReadOnlyList<PrinterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMyPrintersQuery(CurrentProfileId), ct);
        return ToActionResult(result);
    }

    /// <summary>Registers a new printer (created in PendingVerification status).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PrinterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Add([FromBody] AddPrinterRequest request, CancellationToken ct)
    {
        var command = new AddPrinterCommand(
            CurrentProfileId,
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
            request.IsDeliverable,
            request.Materials);

        var result = await Mediator.Send(command, ct);
        if (result.IsFailure)
            return Problem(result.Error);

        return CreatedAtAction(nameof(GetMine), new { }, result.Value);
    }

    /// <summary>Updates a printer's availability (Active / Paused / Decommissioned).</summary>
    [HttpPut("{id:guid}/availability")]
    [ProducesResponseType(typeof(PrinterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvailability(
        Guid id, [FromBody] UpdateAvailabilityRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new UpdatePrinterAvailabilityCommand(id, CurrentProfileId, request.Status, request.MaxConcurrentJobs),
            ct);
        return ToActionResult(result);
    }

    /// <summary>Verifies a printer and assigns a certification level (admin/back-office).</summary>
    [HttpPost("{id:guid}/verify")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PrinterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Verify(
        Guid id, [FromBody] VerifyPrinterRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new VerifyPrinterCommand(id, request.CertificationLevel), ct);
        return ToActionResult(result);
    }

    /// <summary>Returns printers ranked for a given job (best match first).</summary>
    [HttpGet("available-for-job/{jobId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<RankedPrinterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AvailableForJob(Guid jobId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAvailablePrintersForJobQuery(jobId), ct);
        return ToActionResult(result);
    }
}

// ---------------------------------------------------------------------------
// Request DTOs
// ---------------------------------------------------------------------------

public sealed record AddPrinterRequest(
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
    IReadOnlyList<MaterialAttachment>? Materials);

public sealed record UpdateAvailabilityRequest(PrinterStatus Status, int? MaxConcurrentJobs);

public sealed record VerifyPrinterRequest(int CertificationLevel);
