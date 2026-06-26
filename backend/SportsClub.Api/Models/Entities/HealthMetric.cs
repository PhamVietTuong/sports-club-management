namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A member's self-recorded health snapshot (maps to the <c>health_metrics</c>
/// table) — weight/height/body-fat on a given date.
/// </summary>
public class HealthMetric
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public DateOnly RecordedDate { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? BodyFatPct { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
