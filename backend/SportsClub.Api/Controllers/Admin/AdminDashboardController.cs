using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Repositories;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = UserRole.Admin)]
public class AdminDashboardController : ControllerBase
{
    private readonly MemberRepository _members;
    private readonly CoachRepository _coaches;
    private readonly ClassRepository _classes;

    public AdminDashboardController(MemberRepository members, CoachRepository coaches, ClassRepository classes)
    {
        _members = members;
        _coaches = coaches;
        _classes = classes;
    }

    [HttpGet]
    public async Task<ActionResult<AdminStatsDto>> Get() => new AdminStatsDto(
        await _members.CountAllAsync(),
        await _coaches.CountAllAsync(),
        await _classes.CountActiveAsync());
}
