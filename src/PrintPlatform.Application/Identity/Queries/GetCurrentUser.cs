using MediatR;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Application.Identity.DTOs;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Queries;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<Result<CurrentUserDto>>;

public sealed class GetCurrentUserHandler
    : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserDto>>
{
    private readonly IIdentityRepository _repo;

    public GetCurrentUserHandler(IIdentityRepository repo) => _repo = repo;

    public async Task<Result<CurrentUserDto>> Handle(GetCurrentUserQuery query, CancellationToken ct)
    {
        var user = await _repo.GetUserByIdAsync(query.UserId, ct);
        if (user is null)
            return Result.Failure<CurrentUserDto>(IdentityErrors.UserNotFound);

        return Result.Success(new CurrentUserDto(
            user.Id, user.Email, user.PhoneNumber,
            user.FullNameAr, user.FullNameEn,
            user.Role, user.Lang, user.IsActive, user.IsVerified));
    }
}
