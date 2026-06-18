using PrintPlatform.Domain.Identity;

namespace PrintPlatform.Application.Identity.DTOs;

/// <summary>Result returned by login / refresh — tokens plus minimal user info.</summary>
public sealed record AuthResultDto(
    Guid UserId,
    string Email,
    UserRole Role,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

/// <summary>Result of a registration command (pending OTP verification).</summary>
public sealed record RegistrationResultDto(
    Guid UserId,
    string Email,
    string PhoneNumber,
    UserRole Role,
    bool OtpSent);

/// <summary>Current authenticated user projection.</summary>
public sealed record CurrentUserDto(
    Guid UserId,
    string Email,
    string PhoneNumber,
    string FullNameAr,
    string FullNameEn,
    UserRole Role,
    Lang Lang,
    bool IsActive,
    bool IsVerified);

/// <summary>Printer owner profile projection (sensitive fields masked).</summary>
public sealed record PrinterOwnerProfileDto(
    Guid UserId,
    string BusinessName,
    string NationalIdMasked,
    string? InstapayNumber,
    CertificationLevel CertificationLevel,
    decimal CertificationScore,
    string? GamificationPlayerId,
    bool IsAvailable,
    bool OnboardingCompleted,
    DateTimeOffset? OnboardingCompletedAt);
