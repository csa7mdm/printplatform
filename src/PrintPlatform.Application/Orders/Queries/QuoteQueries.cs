using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Orders.Queries;

/// <summary>Lists quotes for the calling customer, newest first.</summary>
public sealed record GetMyQuotesQuery(Guid CustomerUserId)
    : IRequest<Result<IReadOnlyList<QuoteDto>>>;

public sealed class GetMyQuotesHandler
    : IRequestHandler<GetMyQuotesQuery, Result<IReadOnlyList<QuoteDto>>>
{
    private readonly IAppDbContext _db;

    public GetMyQuotesHandler(IAppDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<QuoteDto>>> Handle(
        GetMyQuotesQuery request, CancellationToken cancellationToken)
    {
        var quotes = await _db.Quotes
            .AsNoTracking()
            .Join(
                _db.QuoteRequests.AsNoTracking(),
                quote => quote.QuoteRequestId,
                quoteRequest => quoteRequest.Id,
                (quote, quoteRequest) => new { quote, quoteRequest })
            .Where(x => x.quoteRequest.CustomerUserId == request.CustomerUserId)
            .OrderByDescending(x => x.quote.ValidUntil)
            .Select(x => ModelFileMapper.ToDto(x.quote))
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyList<QuoteDto>>(quotes);
    }
}

/// <summary>Returns a single quote owned by the calling customer.</summary>
public sealed record GetQuoteDetailsQuery(Guid QuoteId, Guid CustomerUserId)
    : IRequest<Result<QuoteDto>>;

public sealed class GetQuoteDetailsHandler
    : IRequestHandler<GetQuoteDetailsQuery, Result<QuoteDto>>
{
    private readonly IAppDbContext _db;

    public GetQuoteDetailsHandler(IAppDbContext db) => _db = db;

    public async Task<Result<QuoteDto>> Handle(
        GetQuoteDetailsQuery request, CancellationToken cancellationToken)
    {
        var quote = await _db.Quotes
            .AsNoTracking()
            .Join(
                _db.QuoteRequests.AsNoTracking(),
                q => q.QuoteRequestId,
                qr => qr.Id,
                (q, qr) => new { Quote = q, QuoteRequest = qr })
            .FirstOrDefaultAsync(
                x => x.Quote.Id == request.QuoteId
                     && x.QuoteRequest.CustomerUserId == request.CustomerUserId,
                cancellationToken);

        if (quote is null)
            return Result.Failure<QuoteDto>(OrderErrors.QuoteNotFound);

        return ModelFileMapper.ToDto(quote.Quote);
    }
}
