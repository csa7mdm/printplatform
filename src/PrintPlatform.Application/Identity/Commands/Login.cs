using FluentValidation;
using MediatR;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Application.Identity.DTOs;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Commands;

public sealed record LoginCommand(string Email, string Password)
    : IRequest<Result<AuthResultDto>>;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResultDto>>
{
    private readonly IIdentityRepository _repo;
    private readonly IUserCredentialStore _credentials;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IRefreshTokenFactory _refreshFactory;
    private readonly ICurrentUserAccessor _current;

    public LoginHandler(
        IIdentityRepository repo,
        IUserCredentialStore credentials,
        IJwtTokenGenerator jwt,
        IRefreshTokenFactory refreshFactory,
        ICurrentUserAccessor current)
    {
        _repo = repo;
        _credentials = credentials;
        _jwt = jwt;
        _refreshFactory = refreshFactory;
        _current = current;
    }

    public async Task<Result<AuthResultDto>> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var email = cmd.Email.Trim().ToLowerInvariant();

        var validation = await _credentials.ValidateCredentialsAsync(email, cmd.Password, ct);
        if (validation.IsFailure)
            return Result.Failure<AuthResultDto>(IdentityErrors.InvalidCredentials);

        var user = await _repo.GetUserByIdAsync(validation.Value, ct);
        if (user is null)
            return Result.Failure<AuthResultDto>(IdentityErrors.InvalidCredentials);
        if (!user.IsActive)
            return Result.Failure<AuthResultDto>(IdentityErrors.AccountInactive);

        var (accessToken, accessExpiry) = _jwt.GenerateAccessToken(user);
        var (rawRefresh, refreshExpiry) = _refreshFactory.Generate();
        var ip = _current.IpAddress ?? "unknown";

        user.AddRefreshToken(RefreshToken.Create(_refreshFactory.Hash(rawRefresh), refreshExpiry, ip));
        await _repo.SaveChangesAsync(ct);

        return Result.Success(new AuthResultDto(
            user.Id, user.Email, user.Role,
            accessToken, accessExpiry, rawRefresh, refreshExpiry));
    }
}
