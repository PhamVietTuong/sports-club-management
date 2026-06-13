using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;

namespace SportsClub.Api.Controllers;

[ApiController]
[Route("api/coach")]
[Authorize(Roles = UserRole.Coach)]
public class CoachController : ControllerBase
{
    private readonly CoachRepository _coaches;
    private readonly ClassRepository _classes;
    private readonly ScheduleRepository _schedules;
    private readonly EnrollmentRepository _enrollments;

    public CoachController(CoachRepository coaches, ClassRepository classes,
        ScheduleRepository schedules, EnrollmentRepository enrollments)
    {
        _coaches = coaches;
        _classes = classes;
        _schedules = schedules;
        _enrollments = enrollments;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));

        var myClasses = await _classes.FindByCoachIdAsync(coach.Id);

        // ITERATOR PATTERN — iterate the coach's schedules
        var schedules = ClubCollection<Schedule>.Of(
            await _schedules.FindByCoachIdAsync(coach.Id));

        return Ok(new
        {
            Coach = CoachDto.From(coach),
            Classes = myClasses.Select(ClassDto.From),
            Schedules = schedules.Select(ScheduleDto.From),
        });
    }

    [HttpGet("classes")]
    public async Task<IActionResult> MyClasses()
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));
        var myClasses = await _classes.FindByCoachIdAsync(coach.Id);
        return Ok(myClasses.Select(ClassDto.From));
    }

    /// <summary>
    /// BROKEN ACCESS CONTROL / IDOR PREVENTION — only expose a class (and its
    /// enrolled members) if it actually belongs to the logged-in coach.
    /// </summary>
    [HttpGet("classes/{id:int}")]
    public async Task<IActionResult> ClassDetail(int id)
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));

        var selected = await _classes.FindByIdAsync(id);
        if (selected is null || selected.CoachId != coach.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền xem lớp học này."));

        var enrolled = await _enrollments.FindActiveByClassIdAsync(id);
        return Ok(new
        {
            Class = ClassDto.From(selected),
            EnrolledMembers = enrolled.Select(EnrollmentDto.From),
        });
    }
}
