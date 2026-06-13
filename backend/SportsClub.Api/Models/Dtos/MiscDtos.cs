using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record EnrollmentDto(
    int Id,
    int MemberId,
    string MemberName,
    int ClassId,
    string ClassName,
    DateOnly EnrollDate,
    string Status)
{
    public static EnrollmentDto From(Enrollment e) => new(
        e.Id, e.MemberId, e.Member?.FullName ?? "", e.ClassId,
        e.Class?.Name ?? "", e.EnrollDate, e.Status);
}

public record AdminStatsDto(int TotalMembers, int TotalCoaches, int TotalClasses);

public record UpdateProfileRequest(
    [param: Required] string FullName,
    string? Phone,
    string? Address,
    string? CurrentPassword,
    string? NewPassword);

/// <summary>Standard error envelope so the SPA can show a single message.</summary>
public record MessageResponse(string Message);
