using MediatR;
using PrintPlatform.Domain.Shared;
using PrintPlatform.Loyalty.Abstractions;

namespace PrintPlatform.Application.Loyalty.Commands;

public sealed record ProcessLoyaltyExpiryCommand() : IRequest<Result<int>>;

public sealed class ProcessLoyaltyExpiryCommandHandler : IRequestHandler<ProcessLoyaltyExpiryCommand, Result<int>>
{
    private readonly ILoyaltyService _loyaltyService;

    public ProcessLoyaltyExpiryCommandHandler(ILoyaltyService loyaltyService)
    {
        _loyaltyService = loyaltyService;
    }

    public async Task<Result<int>> Handle(ProcessLoyaltyExpiryCommand request, CancellationToken cancellationToken)
    {
        var expiredPointsCount = await _loyaltyService.ProcessExpiriesAsync(cancellationToken);
        return Result.Success(expiredPointsCount);
    }
}
