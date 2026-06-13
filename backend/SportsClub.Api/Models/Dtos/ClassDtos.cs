using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record ClassDto(
    int Id,
    string Name,
    int? CoachId,
    string? CoachName,
    int Capacity,
    int CurrentEnrolled,
    int AvailableSlots,
    string? Level,
    string? Description,
    bool IsActive)
{
    public static ClassDto From(TrainingClass c) => new(
        c.Id, c.Name, c.CoachId, c.Coach?.FullName, c.Capacity, c.CurrentEnrolled,
        c.AvailableSlots, c.Level, c.Description, c.IsActive);
}

public record CreateClassRequest(
    [param: Required] string Name,
    int CoachId,
    int Capacity,
    string? Level,
    string? Description);

public record UpdateClassRequest(
    [param: Required] string Name,
    int CoachId,
    int Capacity,
    string? Level,
    string? Description,
    bool IsActive);
