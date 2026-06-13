using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;

namespace SportsClub.Api.Controllers;

[ApiController]
[Route("api/member")]
[Authorize(Roles = UserRole.Member)]
public class MemberController : ControllerBase
{
    private readonly MemberRepository _members;
    private readonly UserRepository _users;
    private readonly ClassRepository _classes;
    private readonly ScheduleRepository _schedules;
    private readonly EnrollmentRepository _enrollments;
    private readonly PackageRepository _packages;

    public MemberController(MemberRepository members, UserRepository users,
        ClassRepository classes, ScheduleRepository schedules,
        EnrollmentRepository enrollments, PackageRepository packages)
    {
        _members = members;
        _users = users;
        _classes = classes;
        _schedules = schedules;
        _enrollments = enrollments;
        _packages = packages;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var enrollments = await _enrollments.FindByMemberIdAsync(member.Id);

        // ITERATOR PATTERN — iterate the member's personal schedule
        var schedules = ClubCollection<Schedule>.Of(await _schedules.FindByMemberIdAsync(member.Id));

        return Ok(new
        {
            Member = MemberDto.From(member),
            Enrollments = enrollments.Select(EnrollmentDto.From),
            Schedules = schedules.Select(ScheduleDto.From),
        });
    }

    [HttpGet("classes")]
    public async Task<IActionResult> AvailableClasses()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var myActive = (await _enrollments.FindByMemberIdAsync(member.Id))
            .Where(e => e.Status == "ACTIVE").Select(e => e.ClassId).ToHashSet();

        // ITERATOR PATTERN — traverse active classes via the club iterator
        var classes = ClubCollection<TrainingClass>.Of(await _classes.FindActiveAsync());

        return Ok(classes.Select(c => new
        {
            Class = ClassDto.From(c),
            IsEnrolled = myActive.Contains(c.Id),
        }));
    }

    [HttpPost("classes/{id:int}/enroll")]
    public async Task<IActionResult> Enroll(int id)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var tc = await _classes.FindByIdAsync(id);
        if (tc is null || !tc.IsActive)
            return BadRequest(new MessageResponse("Lớp học không khả dụng."));
        if (await _enrollments.IsEnrolledAsync(member.Id, id))
            return BadRequest(new MessageResponse("Bạn đã đăng ký rồi."));

        // Atomically reserve a seat. This is the real capacity gate: a single
        // conditional UPDATE that only succeeds if there is room, so two
        // concurrent requests for the last seat cannot both win.
        if (!await _classes.TryIncrementEnrolledAsync(id))
            return BadRequest(new MessageResponse("Lớp học đã đầy."));

        try
        {
            var newlyEnrolled = await _enrollments.InsertAsync(member.Id, id);
            if (!newlyEnrolled)
            {
                // A concurrent request already activated this member — give the seat back.
                await _classes.DecrementEnrolledAsync(id);
                return BadRequest(new MessageResponse("Bạn đã đăng ký rồi."));
            }
        }
        catch
        {
            await _classes.DecrementEnrolledAsync(id); // compensate so the count stays correct
            throw;
        }

        return Ok(new MessageResponse("Đăng ký thành công!"));
    }

    [HttpPost("classes/{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        // CancelAsync returns true only if an ACTIVE enrollment was actually
        // cancelled, so the seat is released exactly once.
        if (!await _enrollments.CancelAsync(member.Id, id))
            return BadRequest(new MessageResponse("Bạn chưa đăng ký lớp này."));

        await _classes.DecrementEnrolledAsync(id);
        return Ok(new MessageResponse("Đã hủy đăng ký."));
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        return Ok(new
        {
            Member = MemberDto.From(member),
            Packages = (await _packages.FindActiveAsync()).Select(PackageDto.From),
        });
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.FullName))
            return BadRequest(new MessageResponse("Họ tên là bắt buộc."));

        var userId = User.GetUserId();
        var member = await _members.FindByUserIdAsync(userId);
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        member.FullName = req.FullName.Trim();
        member.Address = req.Address;
        await _members.UpdateAsync(member);

        // Update the phone (on the users table) only when the field is present
        // in the request; null means "not supplied" and must not wipe it. An
        // empty/blank string is treated as an explicit clear.
        if (req.Phone is not null)
            await _users.UpdatePhoneAsync(userId, string.IsNullOrWhiteSpace(req.Phone) ? null : req.Phone.Trim());

        if (!string.IsNullOrWhiteSpace(req.NewPassword))
        {
            // PASSWORD CHANGE — require and verify the current password so a
            // hijacked/unattended session cannot silently reset it. The user
            // row is only loaded on this branch.
            var user = await _users.FindByIdAsync(userId);
            if (user is null
                || string.IsNullOrEmpty(req.CurrentPassword)
                || !PasswordHasher.Verify(req.CurrentPassword, user.PasswordHash))
                return BadRequest(new MessageResponse("Mật khẩu hiện tại không đúng."));

            var pwError = PasswordPolicy.Validate(req.NewPassword);
            if (pwError is not null) return BadRequest(new MessageResponse(pwError));

            await _users.UpdatePasswordAsync(userId, PasswordHasher.Hash(req.NewPassword));
        }

        return Ok(new MessageResponse("Cập nhật hồ sơ thành công."));
    }
}
