using Microsoft.AspNetCore.DataProtection;
using PrintPlatform.Application.Identity.Abstractions;

namespace PrintPlatform.Infrastructure.Identity;

/// <summary>
/// Encrypts sensitive PII columns (national id, bank details) using ASP.NET
/// Core Data Protection with a dedicated, isolated purpose.
/// </summary>
public sealed class FieldEncryptor : IFieldEncryptor
{
    private const string Purpose = "PrintPlatform.Identity.PII.v1";
    private readonly IDataProtector _protector;

    public FieldEncryptor(IDataProtectionProvider provider)
        => _protector = provider.CreateProtector(Purpose);

    public string Encrypt(string plaintext) => _protector.Protect(plaintext);

    public string Decrypt(string ciphertext) => _protector.Unprotect(ciphertext);
}
