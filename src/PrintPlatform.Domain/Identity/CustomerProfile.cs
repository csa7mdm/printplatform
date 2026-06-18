using PrintPlatform.Domain.Shared;

namespace PrintPlatform.Domain.Identity;

/// <summary>
/// 1:1 extension of a <see cref="User"/> with <see cref="UserRole.Customer"/>.
/// Holds shipping defaults, the linked loyalty membership id and payment preference.
/// </summary>
public sealed class CustomerProfile : BaseEntity<Guid>
{
    private CustomerProfile() { }

    private CustomerProfile(Guid userId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
    }

    /// <summary>FK to the owning <see cref="User"/> (1:1).</summary>
    public Guid UserId { get; private set; }

    /// <summary>Default shipping address (FK into the Orders/Address module). Nullable until set.</summary>
    public Guid? DefaultAddressId { get; private set; }

    /// <summary>External membership id in the reusable Loyalty engine.</summary>
    public string? LoyaltyMemberId { get; private set; }

    /// <summary>Preferred payment method key, e.g. "instapay", "card", "cod".</summary>
    public string? PreferredPaymentMethod { get; private set; }

    public uint RowVersion { get; private set; }

    public static CustomerProfile Create(Guid userId) => new(userId);

    public void SetDefaultAddress(Guid addressId)
    {
        DefaultAddressId = addressId;
        Touch();
    }

    public void LinkLoyaltyMember(string loyaltyMemberId)
    {
        LoyaltyMemberId = loyaltyMemberId;
        Touch();
    }

    public void SetPreferredPaymentMethod(string method)
    {
        PreferredPaymentMethod = method;
        Touch();
    }
}
