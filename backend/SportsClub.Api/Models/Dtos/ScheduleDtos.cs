using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record ScheduleDto(
    int Id,
    int ClassId,
    string ClassName,
    string DayOfWeek,
    string StartTime,
    string EndTime,
    string? Room,
    bool RepeatWeekly)
{
    public static ScheduleDto From(Schedule s) => new(
        s.Id, s.ClassId, s.Class?.Name ?? "", s.DayOfWeek,
        s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"),
        s.Room, s.RepeatWeekly);
}

public record CreateScheduleRequest(
    int ClassId,
    [param: Required] string DayOfWeek,
    [param: Required] string StartTime,
    [param: Required] string EndTime,
    string? Room);
