using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Orders.Commands;

/// <summary>
/// Creates a <see cref="QuoteRequest"/> for an analysed model file. The request enters
/// <see cref="QuoteRequestStatus.PendingReview"/> for operator pricing.
/// </summary>
public sealed record CreateQuoteRequestCommand(
    Guid CustomerUserId,
    Guid ModelFileId,
    Guid MaterialOptionId,
    QualityPreset QualityPreset,
    decimal LayerHeightMm,
    int InfillPercent,
    int Quantity,
    bool IncludeDesignService) : IRequest<Result<Guid>>;

public sealed class CreateQuoteRequestValidator : AbstractValidator<CreateQuoteRequestCommand>
{
    public CreateQuoteRequestValidator()
    {
        RuleFor(x => x.CustomerUserId).NotEmpty();
        RuleFor(x => x.ModelFileId).NotEmpty();
        RuleFor(x => x.MaterialOptionId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.InfillPercent).InclusiveBetween(0, 100);
        RuleFor(x => x.LayerHeightMm).GreaterThan(0);
    }
}

public sealed class CreateQuoteRequestHandler
    : IRequestHandler<CreateQuoteRequestCommand, Result<Guid>>
{
    private readonly IAppDbContext _db;

    public CreateQuoteRequestHandler(IAppDbContext db) => _db = db;

    public async Task<Result<Guid>> Handle(
        CreateQuoteRequestCommand request, CancellationToken cancellationToken)
    {
        var model = await _db.ModelFiles
            .FirstOrDefaultAsync(m => m.Id == request.ModelFileId, cancellationToken);

        if (model is null)
            return Result.Failure<Guid>(OrderErrors.ModelFileNotFound);

        if (model.Status != ModelFileStatus.Approved)
            return Result.Failure<Guid>(OrderErrors.ModelNotAnalysed);

        var creation = QuoteRequest.Create(
            request.ModelFileId,
            request.CustomerUserId,
            request.MaterialOptionId,
            request.QualityPreset,
            request.LayerHeightMm,
            request.InfillPercent,
            request.Quantity,
            request.IncludeDesignService);

        if (creation.IsFailure)
            return Result.Failure<Guid>(creation.Error);

        _db.QuoteRequests.Add(creation.Value);
        await _db.SaveChangesAsync(cancellationToken);

        return creation.Value.Id;
    }
}
