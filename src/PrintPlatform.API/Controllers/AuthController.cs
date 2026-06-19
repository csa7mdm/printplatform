using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Identity.Commands;
using PrintPlatform.Application.Identity.DTOs;
using PrintPlatform.Domain.Identity;

namespace PrintPlatform.API.Controllers;

[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ApiControllerBase
{
    /// <summary>Registers a customer account and sends an OTP to the provided phone number.</summary>
    [AllowAnonymous]
    [HttpPost("register/customer")]
    [ProducesResponseType(typeof(RegistrationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterCustomer(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RegisterCustomerCommand(
                request.Email,
                request.PhoneNumber,
                request.Password,
                request.FullNameAr,
                request.FullNameEn,
                request.Lang),
            ct);

        return ToActionResult(result);
    }

    /// <summary>Registers a printer-owner account and sends an OTP to the provided phone number.</summary>
    [AllowAnonymous]
    [HttpPost("register/printer-owner")]
    [ProducesResponseType(typeof(RegistrationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterPrinterOwner(
        [FromBody] RegisterPrinterOwnerRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RegisterPrinterOwnerCommand(
                request.Email,
                request.PhoneNumber,
                request.Password,
                request.FullNameAr,
                request.FullNameEn,
                request.NationalId,
                request.BusinessName,
                request.Lang),
            ct);

        return ToActionResult(result);
    }

    /// <summary>Authenticates a user and issues access and refresh tokens.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new LoginCommand(request.Email, request.Password), ct);
        return ToActionResult(result);
    }

    /// <summary>Rotates a refresh token and returns a fresh access token pair.</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);
        return ToActionResult(result);
    }

    /// <summary>Revokes a refresh token.</summary>
    [Authorize]
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Revoke([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new RevokeTokenCommand(request.RefreshToken), ct);
        return ToActionResult(result);
    }

    /// <summary>Verifies a phone OTP for a registered user.</summary>
    [AllowAnonymous]
    [HttpPost("verify-otp")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyPhoneOtpRequest request, CancellationToken ct)
    {
        var result = await Mediator.Send(new VerifyPhoneOtpCommand(request.UserId, request.Code), ct);
        return ToActionResult(result);
    }
}

public sealed record RegisterCustomerRequest(
    string Email,
    string PhoneNumber,
    string Password,
    string FullNameAr,
    string FullNameEn,
    Lang Lang);

public sealed record RegisterPrinterOwnerRequest(
    string Email,
    string PhoneNumber,
    string Password,
    string FullNameAr,
    string FullNameEn,
    string NationalId,
    string BusinessName,
    Lang Lang);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record VerifyPhoneOtpRequest(Guid UserId, string Code);
