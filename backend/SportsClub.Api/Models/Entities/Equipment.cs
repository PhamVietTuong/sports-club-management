namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A piece of gym equipment (maps to the <c>equipment</c> table).
/// Managed by the admin only.
/// </summary>
public class Equipment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int Quantity { get; set; } = 1;

    /// <summary>AVAILABLE / IN_USE / MAINTENANCE / RETIRED.</summary>
    public string Status { get; set; } = "AVAILABLE";
    public DateOnly? PurchaseDate { get; set; }
    public string? Notes { get; set; }
}
