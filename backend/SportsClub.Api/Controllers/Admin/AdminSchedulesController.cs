using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/schedules")]
[Authorize(Roles = UserRole.Admin)]
public class AdminSchedulesController : ControllerBase
{
    private readonly ScheduleRepository _schedules;

    public AdminSchedulesController(ScheduleRepository schedules) => _schedules = schedules;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ScheduleDto>>> List()
    {
        // ITERATOR PATTERN — traverse schedules via the club iterator
        var schedules = ClubCollection<Schedule>.Of(await _schedules.FindAllAsync());
        return Ok(schedules.Select(ScheduleDto.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateScheduleRequest req)
    {
        if (!TimeOnly.TryParse(req.StartTime, out var start) ||
            !TimeOnly.TryParse(req.EndTime, out var end))
            return BadRequest(new MessageResponse("Giờ bắt đầu/kết thúc không hợp lệ."));
        if (end <= start)
            return BadRequest(new MessageResponse("Giờ kết thúc phải sau giờ bắt đầu."));

        // Reject a slot that overlaps another session of the same class on the
        // same weekday (e.g. two 07:00–08:00 Monday slots).
        var existing = await _schedules.FindByClassIdAsync(req.ClassId);
        if (Overlaps(existing, req.DayOfWeek, start, end))
            return BadRequest(new MessageResponse(
                "Lịch tập bị trùng giờ với một buổi khác của lớp này trong cùng ngày."));

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

        // Same overlap guard as Create, ignoring the row being edited.
        var existing = await _schedules.FindByClassIdAsync(req.ClassId);
        if (Overlaps(existing, req.DayOfWeek, start, end, excludeId: id))
            return BadRequest(new MessageResponse(
                "Lịch tập bị trùng giờ với một buổi khác của lớp này trong cùng ngày."));

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

    /// <summary>True if any existing slot (other than <paramref name="excludeId"/>)
    /// is on the same weekday and overlaps [start, end).</summary>
    private static bool Overlaps(
        IEnumerable<Schedule> existing, string day, TimeOnly start, TimeOnly end, int excludeId = 0) =>
        existing.Any(s => s.Id != excludeId && s.DayOfWeek == day
                          && s.StartTime < end && start < s.EndTime);
}
