using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Domain.Dispatch;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Dispatch.Queries;

public record GetMyJobsQuery(Guid CurrentUserId) : IRequest<Result<IReadOnlyList<JobAssignmentDto>>>;
public record GetPendingDispatchQuery() : IRequest<Result<IReadOnlyList<JobAssignmentDto>>>;

public class GetMyJobsQueryHandler : IRequestHandler<GetMyJobsQuery, Result<IReadOnlyList<JobAssignmentDto>>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetMyJobsQueryHandler(IAppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<JobAssignmentDto>>> Handle(GetMyJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _dbContext.JobAssignments
            .Where(j => j.PrinterOwnerUserId == request.CurrentUserId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<IReadOnlyList<JobAssignmentDto>>(jobs);
        return Result.Success(dtos);
    }
}

public class GetPendingDispatchQueryHandler : IRequestHandler<GetPendingDispatchQuery, Result<IReadOnlyList<JobAssignmentDto>>>
{
    private readonly IAppDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetPendingDispatchQueryHandler(IAppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<JobAssignmentDto>>> Handle(GetPendingDispatchQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _dbContext.JobAssignments
            .Where(j => j.Status == JobAssignmentStatus.Offered)
            .OrderBy(j => j.OfferedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<JobAssignmentDto>>(jobs);
        return Result.Success(dtos);
    }
}
