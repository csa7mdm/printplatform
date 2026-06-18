namespace PrintPlatform.Domain.Shared;

/// <summary>
/// Marks a domain entity that supports soft-deletion.
/// </summary>
public interface ISoftDelete
{
    bool IsDeleted { get; }
    void SoftDelete();
}
