using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;
using SportsClub.Api.Services;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/members")]
[Authorize(Roles = UserRole.Admin)]
public class AdminMembersController : ControllerBase
{
    private readonly MemberRepository _members;
    private readonly AccountService _account;

    public AdminMembersController(MemberRepository members, AccountService account)
    {
        _members = members;
        _account = account;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> List([FromQuery] string? status)
    {
        var source = string.IsNullOrEmpty(status)
            ? await _members.FindAllAsync()
            : await _members.FindByStatusAsync(status);

        // ITERATOR PATTERN — traverse members via the club iterator
        var members = ClubCollection<Member>.Of(source);
        return Ok(members.Select(MemberDto.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMemberRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Email)
            || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.FullName))
            return BadRequest(new MessageResponse("Vui lòng điền đầy đủ các trường bắt buộc."));

        var result = await _account.CreateUserAsync(req.Username, req.Email, req.Password, req.Phone, UserRole.Member);
        if (result.Error is not null)
            return StatusCode(result.StatusCode, new MessageResponse(result.Error));

        await _members.InsertAsync(result.UserId!.Value, req.FullName.Trim(), req.Gender,
            req.DateOfBirth, req.Address, req.PackageId ?? 0, req.ExpiryDate);

        return Ok(new MessageResponse("Đã thêm thành viên."));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateMemberStatusRequest req)
    {
        if (await _members.FindByIdAsync(id) is null)
            return NotFound(new MessageResponse("Không tìm thấy thành viên."));
        await _members.UpdateStatusAsync(id, req.Status);
        return Ok(new MessageResponse("Đã cập nhật trạng thái."));
    }

    /// <summary>
    /// PROTOTYPE PATTERN — clone an existing member as a template for a new one.
    /// The cloned profile is persisted under a freshly-created login.
    /// </summary>
    [HttpPost("{id:int}/clone")]
    public async Task<IActionResult> Clone(int id, CreateMemberRequest req)
    {
        var template = await _members.FindByIdAsync(id);
        if (template is null) return NotFound(new MessageResponse("Không tìm thấy mẫu thành viên."));
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Email)
            || string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.FullName))
            return BadRequest(new MessageResponse("Vui lòng điền đầy đủ các trường bắt buộc."));

        // PROTOTYPE PATTERN — copy the template's profile fields onto the new member.
        var copy = template.Clone();

        var result = await _account.CreateUserAsync(req.Username, req.Email, req.Password, req.Phone, UserRole.Member);
        if (result.Error is not null)
            return StatusCode(result.StatusCode, new MessageResponse(result.Error));

        await _members.InsertAsync(result.UserId!.Value, req.FullName.Trim(), copy.Gender,
            copy.DateOfBirth, copy.Address, copy.PackageId, copy.ExpiryDate);

        return Ok(new MessageResponse("Đã tạo thành viên từ mẫu."));
    }
}
