using FluentValidation;
using MediatR;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Commands;

public sealed record RevokeTokenCommand(string RefreshToken) : IRequest<Result>;

public sealed class RevokeTokenValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenValidator()
        => RuleFor(x => x.RefreshToken).NotEmpty();
}

public sealed class RevokeTokenHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IIdentityRepository _repo;
    private readonly IRefreshTokenFactory _refreshFactory;
    private readonly ICurrentUserAccessor _current;

    public RevokeTokenHandler(
        IIdentityRepository repo,
        IRefreshTokenFactory refreshFactory,
        ICurrentUserAccessor current)
    {
        _repo = repo;
        _refreshFactory = refreshFactory;
        _current = current;
    }

    public async Task<Result> Handle(RevokeTokenCommand cmd, CancellationToken ct)
    {
        var hash = _refreshFactory.Hash(cmd.RefreshToken);

        var user = await _repo.GetUserByRefreshTokenHashAsync(hash, ct);
        if (user is null)
            return Result.Failure(IdentityErrors.InvalidRefreshToken);

        var ip = _current.IpAddress ?? "unknown";
        var result = user.RevokeRefreshToken(hash, ip);
        if (result.IsFailure)
            return result;

        await _repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
