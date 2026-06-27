using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;
using SportsClub.Api.Services;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/coaches")]
[Authorize(Roles = UserRole.Admin)]
public class AdminCoachesController : ControllerBase
{
    private readonly CoachRepository _coaches;
    private readonly AccountService _account;

    public AdminCoachesController(CoachRepository coaches, AccountService account)
    {
        _coaches = coaches;
        _account = account;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CoachDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        var result = await _coaches.FindPagedAsync(page, pageSize, search, status);
        // ITERATOR PATTERN — traverse the page via the club iterator while mapping.
        return Ok(result.MapIterating(CoachDto.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCoachRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FullName))
            return BadRequest(new MessageResponse("Tên huấn luyện viên là bắt buộc."));

        var result = await _account.CreateUserAsync(req.Username, req.Email, req.Password, req.Phone, UserRole.Coach);
        if (result.Error is not null)
            return StatusCode(result.StatusCode, new MessageResponse(result.Error));

        await _coaches.InsertAsync(result.UserId!.Value, req.FullName.Trim(), req.Specialization,
            req.Bio, req.Experience, req.Salary);

        return Ok(new MessageResponse("Đã thêm huấn luyện viên."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCoachRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FullName))
            return BadRequest(new MessageResponse("Tên huấn luyện viên là bắt buộc."));
        var coach = await _coaches.FindByIdAsync(id);
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy huấn luyện viên."));

        coach.FullName = req.FullName.Trim();
        coach.Specialization = req.Specialization;
        coach.Bio = req.Bio;
        coach.Experience = req.Experience;
        coach.Salary = req.Salary;
        await _coaches.UpdateAsync(coach);

        return Ok(new MessageResponse("Đã cập nhật huấn luyện viên."));
    }

    /// <summary>
    /// Update a coach's employment status (ACTIVE / UNDER_REVIEW / TERMINATED).
    /// Used by the admin to flag a coach for review or to fire one who no longer
    /// meets the qualifications.
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateCoachStatusRequest req)
    {
        if (!UpdateCoachStatusRequest.Allowed.Contains(req.Status))
            return BadRequest(new MessageResponse("Trạng thái không hợp lệ."));
        if (await _coaches.FindByIdAsync(id) is null)
            return NotFound(new MessageResponse("Không tìm thấy huấn luyện viên."));
        await _coaches.UpdateStatusAsync(id, req.Status);
        return Ok(new MessageResponse("Đã cập nhật trạng thái."));
    }
}
