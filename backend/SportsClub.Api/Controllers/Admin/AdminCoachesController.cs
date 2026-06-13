using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
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
    public async Task<ActionResult<IEnumerable<CoachDto>>> List() =>
        Ok((await _coaches.FindAllAsync()).Select(CoachDto.From));

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
}
