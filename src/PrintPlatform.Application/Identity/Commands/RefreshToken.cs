using FluentValidation;
using MediatR;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Application.Identity.DTOs;
using DomainRefreshToken = PrintPlatform.Domain.Identity.RefreshToken;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Commands;

public sealed record RefreshTokenCommand(string RefreshToken)
    : IRequest<Result<AuthResultDto>>;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
        => RuleFor(x => x.RefreshToken).NotEmpty();
}

public sealed class RefreshTokenHandler
    : IRequestHandler<RefreshTokenCommand, Result<AuthResultDto>>
{
    private readonly IIdentityRepository _repo;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IRefreshTokenFactory _refreshFactory;
    private readonly ICurrentUserAccessor _current;

    public RefreshTokenHandler(
        IIdentityRepository repo,
        IJwtTokenGenerator jwt,
        IRefreshTokenFactory refreshFactory,
        ICurrentUserAccessor current)
    {
        _repo = repo;
        _jwt = jwt;
        _refreshFactory = refreshFactory;
        _current = current;
    }

    public async Task<Result<AuthResultDto>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        var incomingHash = _refreshFactory.Hash(cmd.RefreshToken);

        var user = await _repo.GetUserByRefreshTokenHashAsync(incomingHash, ct);
        if (user is null)
            return Result.Failure<AuthResultDto>(IdentityErrors.InvalidRefreshToken);
        if (!user.IsActive)
            return Result.Failure<AuthResultDto>(IdentityErrors.AccountInactive);

        var ip = _current.IpAddress ?? "unknown";

        // Rotate: revoke the old token and issue a fresh one.
        var (rawRefresh, refreshExpiry) = _refreshFactory.Generate();
        var replacement = DomainRefreshToken.Create(_refreshFactory.Hash(rawRefresh), refreshExpiry, ip);

        var rotation = user.RotateRefreshToken(incomingHash, replacement, ip);
        if (rotation.IsFailure)
            return Result.Failure<AuthResultDto>(rotation.Error);

        var (accessToken, accessExpiry) = _jwt.GenerateAccessToken(user);
        await _repo.SaveChangesAsync(ct);

        return Result.Success(new AuthResultDto(
            user.Id, user.Email, user.Role,
            accessToken, accessExpiry, rawRefresh, refreshExpiry));
    }
}
