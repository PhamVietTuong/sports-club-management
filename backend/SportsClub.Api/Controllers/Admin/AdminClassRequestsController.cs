using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;
using SportsClub.Api.Services;

namespace SportsClub.Api.Controllers.Admin;

/// <summary>
/// Admin review of coach class-change requests (CLAIM / RELEASE). The class
/// assignment is only applied when the admin approves — the atomic claim/release
/// is performed here, so a stale request safely fails if another coach already
/// took the class.
/// </summary>
[ApiController]
[Route("api/admin/class-requests")]
[Authorize(Roles = UserRole.Admin)]
public class AdminClassRequestsController : ControllerBase
{
    private readonly ClassChangeRequestRepository _requests;
    private readonly ClassRepository _classes;
    private readonly ScheduleRepository _schedules;

    public AdminClassRequestsController(ClassChangeRequestRepository requests,
        ClassRepository classes, ScheduleRepository schedules)
    {
        _requests = requests;
        _classes = classes;
        _schedules = schedules;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClassChangeRequestDto>>> List([FromQuery] string? status)
    {
        // ITERATOR PATTERN — traverse requests via the club iterator
        var requests = ClubCollection<ClassChangeRequest>.Of(await _requests.FindAllAsync(status));
        return Ok(requests.Select(ClassChangeRequestDto.From));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var r = await _requests.FindByIdAsync(id);
        if (r is null) return NotFound(new MessageResponse("Không tìm thấy yêu cầu."));
        if (r.Status != "PENDING")
            return BadRequest(new MessageResponse("Yêu cầu đã được xử lý."));

        // SCHEDULE CLASH GUARD — re-check at the binding moment, since the coach's
        // timetable may have changed since the request was filed.
        if (r.Action == "CLAIM")
        {
            var conflict = ScheduleClash.FindConflict(
                await _schedules.FindByClassIdAsync(r.ClassId),
                await _schedules.FindByCoachIdAsync(r.CoachId));
            if (conflict is { } c)
                return BadRequest(new MessageResponse(
                    $"Không thể duyệt — lịch trùng với lớp \"{c.existing.Class?.Name}\" " +
                    $"({c.incoming.DayOfWeek} {c.incoming.StartTime:HH\\:mm}–{c.incoming.EndTime:HH\\:mm})."));
        }

        // Apply the change atomically. A claim only succeeds if the class is still
        // active & unassigned; a release only if this coach still owns it.
        var applied = r.Action == "CLAIM"
            ? await _classes.TryClaimAsync(r.ClassId, r.CoachId)
            : await _classes.ReleaseAsync(r.ClassId, r.CoachId);
        if (!applied)
            return BadRequest(new MessageResponse(r.Action == "CLAIM"
                ? "Không thể gán lớp — lớp đã có HLV khác hoặc không còn khả dụng."
                : "Không thể trả lớp — HLV không còn phụ trách lớp này."));

        r.Status = "APPROVED";
        r.DecidedAt = DateTime.Now;
        await _requests.UpdateAsync(r);
        return Ok(new MessageResponse(r.Action == "CLAIM" ? "Đã duyệt nhận lớp." : "Đã duyệt trả lớp."));
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, DecisionRequest req)
    {
        var r = await _requests.FindByIdAsync(id);
        if (r is null) return NotFound(new MessageResponse("Không tìm thấy yêu cầu."));
        if (r.Status != "PENDING")
            return BadRequest(new MessageResponse("Yêu cầu đã được xử lý."));

        r.Status = "REJECTED";
        r.DecidedAt = DateTime.Now;
        r.Note = req.Note;
        await _requests.UpdateAsync(r);
        return Ok(new MessageResponse("Đã từ chối yêu cầu."));
    }
}
