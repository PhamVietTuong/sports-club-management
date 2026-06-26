namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A payment made by a member (maps to the <c>payments</c> table). Created when a
/// member buys a membership package or pays a fee; revenue reports aggregate this
/// table.
/// </summary>
public class Payment
{
    public int Id { get; set; }
    public int MemberId { get; set; }

    /// <summary>The package this payment bought, when applicable (0/null otherwise).</summary>
    public int? PackageId { get; set; }
    public decimal Amount { get; set; }

    /// <summary>CASH / CARD / TRANSFER.</summary>
    public string Method { get; set; } = "CASH";

    /// <summary>PENDING / COMPLETED / REFUNDED.</summary>
    public string Status { get; set; } = "COMPLETED";
    public string? Description { get; set; }
    public DateTime PaidAt { get; set; }

    // Navigation
    public Member Member { get; set; } = null!;
}
