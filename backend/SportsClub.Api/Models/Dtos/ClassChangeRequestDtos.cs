using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record ClassChangeRequestDto(
    int Id,
    int CoachId,
    string CoachName,
    int ClassId,
    string ClassName,
    string Action,
    string Status,
    DateTime RequestedAt,
    DateTime? DecidedAt,
    string? Note)
{
    public static ClassChangeRequestDto From(ClassChangeRequest r) => new(
        r.Id, r.CoachId, r.Coach?.FullName ?? "", r.ClassId, r.Class?.Name ?? "",
        r.Action, r.Status, r.RequestedAt, r.DecidedAt, r.Note);
}
