namespace PrintPlatform.Domain.Shared;

/// <summary>
/// Base class for all domain entities.
/// Provides audit fields and soft-delete support.
/// Aggregate roots should extend <see cref="BaseAggregateRoot{TId}"/> instead.
/// </summary>
public abstract class BaseEntity<TId> : IEntity<TId>, ISoftDelete
    where TId : notnull
{
    public TId Id { get; protected set; } = default!;

    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>Soft-delete flag. Repositories must filter on this by default.</summary>
    public bool IsDeleted { get; private set; }

    /// <summary>Marks the entity as soft-deleted.</summary>
    public virtual void SoftDelete()
    {
        IsDeleted = true;
        Touch();
    }

    /// <summary>Updates the <see cref="UpdatedAt"/> timestamp to now.</summary>
    protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}
