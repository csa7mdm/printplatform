namespace PrintPlatform.Domain.Shared;

/// <summary>
/// Represents a business-logic error without throwing exceptions.
/// </summary>
/// <param name="Code">Machine-readable error code, e.g. <c>"Order.NotFound"</c>.</param>
/// <param name="Message">Human-readable description.</param>
/// <param name="Type">Classification used to map to HTTP status codes in the API layer.</param>
public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    /// <summary>Represents the absence of an error (success path).</summary>
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static Error NotFound(string code, string message)
        => new(code, message, ErrorType.NotFound);

    public static Error Validation(string code, string message)
        => new(code, message, ErrorType.Validation);

    public static Error Conflict(string code, string message)
        => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code, string message)
        => new(code, message, ErrorType.Unauthorized);

    public static Error Failure(string code, string message)
        => new(code, message, ErrorType.Failure);
}

public enum ErrorType
{
    None,
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
}
