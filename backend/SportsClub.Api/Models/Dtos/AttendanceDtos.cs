using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record AttendanceDto(
    int Id,
    int ClassId,
    string ClassName,
    int MemberId,
    string MemberName,
    DateOnly SessionDate,
    string Status,
    DateTime? CheckedInAt)
{
    public static AttendanceDto From(Attendance a) => new(
        a.Id, a.ClassId, a.Class?.Name ?? "", a.MemberId, a.Member?.FullName ?? "",
        a.SessionDate, a.Status, a.CheckedInAt);

    /// <summary>Statuses allowed by the attendance table CHECK constraint.</summary>
    public static readonly string[] AllowedStatuses = { "PRESENT", "ABSENT", "LATE" };
}

/// <summary>One coach-supplied attendance mark for a member on a session date.</summary>
public record MarkAttendanceRequest(
    [param: Required] int MemberId,
    [param: Required] DateOnly SessionDate,
    [param: Required] string Status);

/// <summary>
/// One row of the coach's attendance roster for a class on a date: an enrolled
/// member plus their mark for that day (null status = not yet marked).
/// </summary>
public record AttendanceRosterEntryDto(
    int MemberId,
    string MemberName,
    string? Status,
    DateTime? CheckedInAt);
