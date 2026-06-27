using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;
using SportsClub.Api.Services;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/schedules")]
[Authorize(Roles = UserRole.Admin)]
public class AdminSchedulesController : ControllerBase
{
    private readonly ScheduleRepository _schedules;
    private readonly ClassRepository _classes;

    public AdminSchedulesController(ScheduleRepository schedules, ClassRepository classes)
    {
        _schedules = schedules;
        _classes = classes;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ScheduleDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _schedules.FindPagedAsync(page, pageSize, search);
        // ITERATOR PATTERN — traverse the page via the club iterator while mapping.
        return Ok(result.MapIterating(ScheduleDto.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateScheduleRequest req)
    {
        if (!TimeOnly.TryParse(req.StartTime, out var start) ||
            !TimeOnly.TryParse(req.EndTime, out var end))
            return BadRequest(new MessageResponse("Giờ bắt đầu/kết thúc không hợp lệ."));
        if (end <= start)
            return BadRequest(new MessageResponse("Giờ kết thúc phải sau giờ bắt đầu."));

        var conflict = await ValidateSlotAsync(req.ClassId, req.Room, req.DayOfWeek, start, end, excludeId: 0);
        if (conflict is not null) return BadRequest(new MessageResponse(conflict));

        await _schedules.SaveAsync(new Schedule
        {
            ClassId = req.ClassId,
            DayOfWeek = req.DayOfWeek,
            StartTime = start,
            EndTime = end,
            Room = req.Room,
            RepeatWeekly = true,
        });
        return Ok(new MessageResponse("Đã thêm lịch tập."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateScheduleRequest req)
    {
        var schedule = await _schedules.FindByIdAsync(id);
        if (schedule is null) return NotFound(new MessageResponse("Không tìm thấy lịch tập."));
        if (!TimeOnly.TryParse(req.StartTime, out var start) ||
            !TimeOnly.TryParse(req.EndTime, out var end))
            return BadRequest(new MessageResponse("Giờ bắt đầu/kết thúc không hợp lệ."));
        if (end <= start)
            return BadRequest(new MessageResponse("Giờ kết thúc phải sau giờ bắt đầu."));

        var conflict = await ValidateSlotAsync(req.ClassId, req.Room, req.DayOfWeek, start, end, excludeId: id);
        if (conflict is not null) return BadRequest(new MessageResponse(conflict));

        schedule.ClassId = req.ClassId;
        schedule.DayOfWeek = req.DayOfWeek;
        schedule.StartTime = start;
        schedule.EndTime = end;
        schedule.Room = req.Room;
        await _schedules.UpdateAsync(schedule);

        return Ok(new MessageResponse("Đã cập nhật lịch tập."));
    }

    /// <summary>PROTOTYPE PATTERN — clone this week's schedule into the next week.</summary>
    [HttpPost("{id:int}/clone")]
    public async Task<IActionResult> Clone(int id)
    {
        var thisWeek = await _schedules.FindByIdAsync(id);
        if (thisWeek is null) return NotFound(new MessageResponse("Không tìm thấy lịch tập."));

        var nextWeek = thisWeek.Clone();   // PROTOTYPE in action
        nextWeek.Id = 0;
        nextWeek.Room = (thisWeek.Room ?? "") + " (Copy)";
        nextWeek.Class = null!;
        await _schedules.SaveAsync(nextWeek);

        return Ok(new MessageResponse("Đã nhân bản lịch tập."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _schedules.DeleteAsync(id);
        return Ok(new MessageResponse("Đã xóa lịch tập."));
    }

    /// <summary>
    /// Validates a schedule slot against three rules, returning an error message
    /// (or null when the slot is fine):
    ///   • no overlap with another session of the SAME class on that day;
    ///   • no two classes in the SAME room at once (rule 2);
    ///   • the class's coach is not already teaching another class then (rule 1).
    /// </summary>
    private async Task<string?> ValidateSlotAsync(
        int classId, string? room, string day, TimeOnly start, TimeOnly end, int excludeId)
    {
        var sameClass = await _schedules.FindByClassIdAsync(classId);
        if (sameClass.Any(s => s.Id != excludeId && s.DayOfWeek == day
                               && s.StartTime < end && start < s.EndTime))
            return "Lịch tập bị trùng giờ với một buổi khác của lớp này trong cùng ngày.";

        if (!string.IsNullOrWhiteSpace(room))
        {
            var roomClash = ScheduleClash.FindRoomClash(
                room, day, start, end, await _schedules.FindByRoomAsync(room), excludeId);
            if (roomClash is not null)
                return $"Phòng \"{room}\" đã có lớp \"{roomClash.Class?.Name}\" " +
                       $"vào khung giờ này ({roomClash.StartTime:HH\\:mm}–{roomClash.EndTime:HH\\:mm}).";
        }

        var cls = await _classes.FindByIdAsync(classId);
        if (cls?.CoachId is int coachId)
        {
            var coachSlots = (await _schedules.FindByCoachIdAsync(coachId)).Where(s => s.ClassId != classId);
            var candidate = new Schedule { DayOfWeek = day, StartTime = start, EndTime = end };
            if (ScheduleClash.FindConflict(new[] { candidate }, coachSlots) is { } c)
                return $"HLV đang dạy lớp \"{c.existing.Class?.Name}\" " +
                       $"vào khung giờ này ({c.existing.StartTime:HH\\:mm}–{c.existing.EndTime:HH\\:mm}).";
        }
        return null;
    }
}
