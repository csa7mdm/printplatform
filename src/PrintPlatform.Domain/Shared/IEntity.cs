namespace PrintPlatform.Domain.Shared;

/// <summary>
/// Marks a type as a domain entity with a strongly-typed identifier.
/// </summary>
public interface IEntity<TId>
    where TId : notnull
{
    TId Id { get; }
}
