using MediatR;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Application.Identity.Commands;
using PrintPlatform.Application.Identity.DTOs;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Queries;

public sealed record GetPrinterOwnerProfileQuery(Guid UserId)
    : IRequest<Result<PrinterOwnerProfileDto>>;

public sealed class GetPrinterOwnerProfileHandler
    : IRequestHandler<GetPrinterOwnerProfileQuery, Result<PrinterOwnerProfileDto>>
{
    private readonly IIdentityRepository _repo;

    public GetPrinterOwnerProfileHandler(IIdentityRepository repo) => _repo = repo;

    public async Task<Result<PrinterOwnerProfileDto>> Handle(
        GetPrinterOwnerProfileQuery query, CancellationToken ct)
    {
        var profile = await _repo.GetPrinterOwnerProfileByUserIdAsync(query.UserId, ct);
        if (profile is null)
            return Result.Failure<PrinterOwnerProfileDto>(IdentityErrors.ProfileNotFound);

        return Result.Success(CompletePrinterOwnerOnboardingHandler.MapToDto(profile));
    }
}
