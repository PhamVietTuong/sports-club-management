using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record LessonPlanDto(
    int Id,
    int ClassId,
    string ClassName,
    int CoachId,
    string Title,
    string? Content,
    DateTime CreatedAt)
{
    public static LessonPlanDto From(LessonPlan p) => new(
        p.Id, p.ClassId, p.Class?.Name ?? "", p.CoachId, p.Title, p.Content, p.CreatedAt);
}

public record SaveLessonPlanRequest(
    [param: Required] int ClassId,
    [param: Required] string Title,
    string? Content);

public record ProgressNoteDto(
    int Id,
    int MemberId,
    string MemberName,
    int CoachId,
    int? ClassId,
    string Note,
    int? Rating,
    DateTime RecordedAt)
{
    public static ProgressNoteDto From(ProgressNote n) => new(
        n.Id, n.MemberId, n.Member?.FullName ?? "", n.CoachId, n.ClassId,
        n.Note, n.Rating, n.RecordedAt);
}

public record SaveProgressNoteRequest(
    [param: Required] int MemberId,
    int? ClassId,
    [param: Required] string Note,
    [param: Range(1, 5)] int? Rating);
