using SportsClub.Api.Patterns.Prototype;

namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A training class (maps to the <c>training_classes</c> table).
/// PROTOTYPE PATTERN — clone a class template to create a duplicate.
/// </summary>
public class TrainingClass : ISportClubPrototype<TrainingClass>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? CoachId { get; set; }
    public int Capacity { get; set; } = 20;
    public int CurrentEnrolled { get; set; }
    public string? Level { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Coach? Coach { get; set; }

    public int AvailableSlots => Capacity - CurrentEnrolled;

    public TrainingClass Clone() => (TrainingClass)MemberwiseClone();
}
