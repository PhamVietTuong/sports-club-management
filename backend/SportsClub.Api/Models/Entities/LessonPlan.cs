namespace SportsClub.Api.Models.Entities;

/// <summary>
/// A lesson plan / curriculum (giáo án) a coach attaches to one of their classes
/// (maps to the <c>lesson_plans</c> table). Members enrolled in the class can read it.
/// </summary>
public class LessonPlan
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public int CoachId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation
    public TrainingClass Class { get; set; } = null!;
}
