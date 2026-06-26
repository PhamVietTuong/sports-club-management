using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record PtSessionDto(
    int Id,
    int MemberId,
    string MemberName,
    int CoachId,
    string CoachName,
    DateOnly SessionDate,
    string StartTime,
    string EndTime,
    string Status,
    string? Notes)
{
    public static PtSessionDto From(PtSession s) => new(
        s.Id, s.MemberId, s.Member?.FullName ?? "", s.CoachId, s.Coach?.FullName ?? "",
        s.SessionDate, s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"),
        s.Status, s.Notes);
}

public record BookPtRequest(
    [param: Required] int CoachId,
    [param: Required] DateOnly SessionDate,
    [param: Required] string StartTime,
    [param: Required] string EndTime,
    string? Notes);

/// <summary>Coach-side status transition for a PT session.</summary>
public record UpdatePtStatusRequest([param: Required] string Status)
{
    public static readonly string[] Allowed = { "PENDING", "CONFIRMED", "CANCELLED", "COMPLETED" };
}
