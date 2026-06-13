using SportsClub.Api.Patterns.Prototype;

namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A membership/training package (maps to the <c>training_packages</c> table).
/// PROTOTYPE PATTERN — clone a package template, then adjust (e.g. +20% price).
/// </summary>
public class TrainingPackage : ISportClubPrototype<TrainingPackage>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DurationMonths { get; set; }
    public decimal Price { get; set; }
    public int MaxClasses { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public TrainingPackage Clone() => (TrainingPackage)MemberwiseClone();
}
