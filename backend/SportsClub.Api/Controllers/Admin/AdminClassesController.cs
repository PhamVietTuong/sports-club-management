using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;
using SportsClub.Api.Services;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/classes")]
[Authorize(Roles = UserRole.Admin)]
public class AdminClassesController : ControllerBase
{
    private readonly ClassRepository _classes;
    private readonly ScheduleRepository _schedules;

    public AdminClassesController(ClassRepository classes, ScheduleRepository schedules)
    {
        _classes = classes;
        _schedules = schedules;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ClassDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _classes.FindPagedAsync(page, pageSize, search);
        // ITERATOR PATTERN — traverse the page via the club iterator while mapping.
        return Ok(result.MapIterating(ClassDto.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClassRequest req)
    {
        await _classes.InsertAsync(new TrainingClass
        {
            Name = req.Name,
            CoachId = req.CoachId == 0 ? null : req.CoachId,
            Capacity = req.Capacity,
            Level = req.Level,
            Description = req.Description,
            IsActive = true,
        });
        return Ok(new MessageResponse("Đã thêm lớp học."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateClassRequest req)
    {
        var tc = await _classes.FindByIdAsync(id);
        if (tc is null) return NotFound(new MessageResponse("Không tìm thấy lớp học."));

        // RULE 1 — a coach can't teach two classes at once: if assigning a coach,
        // this class's schedule must not clash with the coach's other classes.
        var newCoachId = req.CoachId == 0 ? (int?)null : req.CoachId;
        if (newCoachId is int coachId)
        {
            var thisClassSlots = await _schedules.FindByClassIdAsync(id);
            var coachSlots = (await _schedules.FindByCoachIdAsync(coachId)).Where(s => s.ClassId != id);
            if (ScheduleClash.FindConflict(thisClassSlots, coachSlots) is { } c)
                return BadRequest(new MessageResponse(
                    $"HLV đang dạy lớp \"{c.existing.Class?.Name}\" trùng giờ với lớp này " +
                    $"({c.incoming.DayOfWeek} {c.incoming.StartTime:HH\\:mm}–{c.incoming.EndTime:HH\\:mm})."));
        }

        tc.Name = req.Name;
        tc.CoachId = newCoachId;
        tc.Capacity = req.Capacity;
        tc.Level = req.Level;
        tc.Description = req.Description;
        tc.IsActive = req.IsActive;
        await _classes.UpdateAsync(tc);

        return Ok(new MessageResponse("Đã cập nhật lớp học."));
    }

    /// <summary>PROTOTYPE PATTERN — duplicate a class template.</summary>
    [HttpPost("{id:int}/clone")]
    public async Task<IActionResult> Clone(int id)
    {
        var template = await _classes.FindByIdAsync(id);
        if (template is null) return NotFound(new MessageResponse("Không tìm thấy lớp học."));

        var copy = template.Clone();   // PROTOTYPE in action
        copy.Id = 0;
        copy.Name = "Copy of " + template.Name;
        copy.CurrentEnrolled = 0;
        copy.Coach = null;
        await _classes.InsertAsync(copy);

        return Ok(new MessageResponse("Đã nhân bản lớp học."));
    }
}
