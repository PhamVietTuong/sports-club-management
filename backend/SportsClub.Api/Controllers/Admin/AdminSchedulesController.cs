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
}
