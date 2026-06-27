using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;

namespace SportsClub.Api.Controllers.Admin;

/// <summary>
/// Admin review of member package-registration requests. Approving a request
/// does NOT charge or activate the membership — it opens a 24h grace window in
/// which the member may still cancel/change. The member (or their first class
/// registration) activates it.
/// </summary>
[ApiController]
[Route("api/admin/membership-requests")]
[Authorize(Roles = UserRole.Admin)]
public class AdminMembershipRequestsController : ControllerBase
{
    private readonly MembershipRequestRepository _requests;

    public AdminMembershipRequestsController(MembershipRequestRepository requests) => _requests = requests;

    [HttpGet]
    public async Task<ActionResult<PagedResult<MembershipRequestDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        var result = await _requests.FindPagedAsync(page, pageSize, search, status);
        // ITERATOR PATTERN — traverse the page via the club iterator while mapping.
        return Ok(result.MapIterating(MembershipRequestDto.From));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var r = await _requests.FindByIdAsync(id);
        if (r is null) return NotFound(new MessageResponse("Không tìm thấy yêu cầu."));
        if (r.Status != "PENDING")
            return BadRequest(new MessageResponse("Chỉ có thể duyệt yêu cầu đang chờ."));

        r.Status = "APPROVED";
        r.ApprovedAt = DateTime.Now;
        await _requests.UpdateAsync(r);
        return Ok(new MessageResponse("Đã duyệt yêu cầu. Thành viên có thể kích hoạt gói tập."));
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, DecisionRequest req)
    {
        var r = await _requests.FindByIdAsync(id);
        if (r is null) return NotFound(new MessageResponse("Không tìm thấy yêu cầu."));
        if (r.Status != "PENDING")
            return BadRequest(new MessageResponse("Chỉ có thể từ chối yêu cầu đang chờ."));

        r.Status = "REJECTED";
        r.Note = req.Note;
        await _requests.UpdateAsync(r);
        return Ok(new MessageResponse("Đã từ chối yêu cầu."));
    }
}
