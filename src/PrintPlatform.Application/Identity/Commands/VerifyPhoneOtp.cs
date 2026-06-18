using FluentValidation;
using MediatR;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Domain.Identity;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Application.Identity.Commands;

public sealed record VerifyPhoneOtpCommand(Guid UserId, string Code) : IRequest<Result>;

public sealed class VerifyPhoneOtpValidator : AbstractValidator<VerifyPhoneOtpCommand>
{
    public VerifyPhoneOtpValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Matches(@"^[0-9]{4,8}$");
    }
}

public sealed class VerifyPhoneOtpHandler : IRequestHandler<VerifyPhoneOtpCommand, Result>
{
    private readonly IIdentityRepository _repo;
    private readonly IOtpSender _otp;

    public VerifyPhoneOtpHandler(IIdentityRepository repo, IOtpSender otp)
    {
        _repo = repo;
        _otp = otp;
    }

    public async Task<Result> Handle(VerifyPhoneOtpCommand cmd, CancellationToken ct)
    {
        var user = await _repo.GetUserByIdAsync(cmd.UserId, ct);
        if (user is null)
            return Result.Failure(IdentityErrors.UserNotFound);

        var ok = await _otp.VerifyAsync(user.PhoneNumber, cmd.Code, ct);
        if (!ok)
            return Result.Failure(IdentityErrors.InvalidOtp);

        var result = user.MarkPhoneVerified();
        if (result.IsFailure)
            return result;

        await _repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
