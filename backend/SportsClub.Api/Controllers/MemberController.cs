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
    private readonly PaymentRepository _payments;
    private readonly AttendanceRepository _attendance;
    private readonly LessonPlanRepository _lessonPlans;
    private readonly ProgressNoteRepository _progress;
    private readonly CoachRepository _coaches;
    private readonly CoachRatingRepository _ratings;
    private readonly HealthMetricRepository _health;
    private readonly PtSessionRepository _pt;

    public MemberController(MemberRepository members, UserRepository users,
        ClassRepository classes, ScheduleRepository schedules,
        EnrollmentRepository enrollments, PackageRepository packages,
        PaymentRepository payments, AttendanceRepository attendance,
        LessonPlanRepository lessonPlans, ProgressNoteRepository progress,
        CoachRepository coaches, CoachRatingRepository ratings,
        HealthMetricRepository health, PtSessionRepository pt)
    {
        _members = members;
        _users = users;
        _classes = classes;
        _schedules = schedules;
        _enrollments = enrollments;
        _packages = packages;
        _payments = payments;
        _attendance = attendance;
        _lessonPlans = lessonPlans;
        _progress = progress;
        _coaches = coaches;
        _ratings = ratings;
        _health = health;
        _pt = pt;
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

    // ── Module 2: Buy membership + payment history ───────────────────────────
    [HttpGet("payments")]
    public async Task<IActionResult> MyPayments()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        return Ok((await _payments.FindByMemberIdAsync(member.Id)).Select(PaymentDto.From));
    }

    [HttpPost("membership/buy")]
    public async Task<IActionResult> BuyMembership(BuyMembershipRequest req)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var method = string.IsNullOrEmpty(req.Method) ? "CASH" : req.Method;
        if (!BuyMembershipRequest.AllowedMethods.Contains(method))
            return BadRequest(new MessageResponse("Phương thức thanh toán không hợp lệ."));

        var pkg = await _packages.FindByIdAsync(req.PackageId);
        if (pkg is null || !pkg.IsActive)
            return BadRequest(new MessageResponse("Gói tập không khả dụng."));

        // Record the payment at the package's listed price.
        await _payments.SaveAsync(new Payment
        {
            MemberId = member.Id,
            PackageId = pkg.Id,
            Amount = pkg.Price,
            Method = method,
            Status = "COMPLETED",
            Description = $"Mua gói {pkg.Name}",
            PaidAt = DateTime.Now,
        });

        // Extend the membership: stack onto the remaining time if not yet expired.
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = member.ExpiryDate is { } exp && exp > today ? exp : today;
        member.PackageId = pkg.Id;
        member.ExpiryDate = start.AddMonths(pkg.DurationMonths);
        member.Status = "ACTIVE";
        await _members.UpdateAsync(member);

        return Ok(new MessageResponse($"Đã mua gói {pkg.Name}. Hạn mới: {member.ExpiryDate:dd/MM/yyyy}."));
    }

    // ── Module 3: Self check-in + attendance history ─────────────────────────
    [HttpPost("classes/{id:int}/checkin")]
    public async Task<IActionResult> CheckIn(int id)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        // Only an actively-enrolled member may check in to a class.
        if (!await _enrollments.IsEnrolledAsync(member.Id, id))
            return BadRequest(new MessageResponse("Bạn chưa đăng ký lớp này."));

        var today = DateOnly.FromDateTime(DateTime.Today);
        await _attendance.UpsertAsync(id, member.Id, today, "PRESENT", checkedIn: true);
        return Ok(new MessageResponse("Đã check-in hôm nay."));
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> MyAttendance()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        return Ok((await _attendance.FindByMemberIdAsync(member.Id)).Select(AttendanceDto.From));
    }

    // ── Module 4: Receive lesson plans + read own progress ───────────────────
    [HttpGet("lesson-plans")]
    public async Task<IActionResult> MyLessonPlans()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        // Only plans for classes the member is actively enrolled in.
        var classIds = (await _enrollments.FindByMemberIdAsync(member.Id))
            .Where(e => e.Status == "ACTIVE").Select(e => e.ClassId).Distinct().ToList();
        var plans = await _lessonPlans.FindByClassIdsAsync(classIds);
        return Ok(plans.Select(LessonPlanDto.From));
    }

    [HttpGet("progress")]
    public async Task<IActionResult> MyProgress()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        return Ok((await _progress.FindByMemberIdAsync(member.Id)).Select(ProgressNoteDto.From));
    }

    // ── Module 5: Coaches list + rate a coach ────────────────────────────────
    [HttpGet("coaches")]
    public async Task<IActionResult> Coaches()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var activeCoaches = await _coaches.FindActiveAsync();
        var aggregates = (await _ratings.AveragesAsync()).ToDictionary(a => a.CoachId);
        var myRatings = (await _ratings.FindByMemberIdAsync(member.Id)).ToDictionary(r => r.CoachId);
        var myCoachIds = await MyCoachIdsAsync(member.Id);

        var list = activeCoaches.Select(c =>
        {
            aggregates.TryGetValue(c.Id, out var agg);
            myRatings.TryGetValue(c.Id, out var mine);
            return new RateableCoachDto(
                c.Id, c.FullName, c.Specialization, c.Experience,
                agg is null ? 0 : Math.Round(agg.Average, 1), agg?.Count ?? 0,
                mine?.Rating, mine?.Comment, myCoachIds.Contains(c.Id));
        });
        return Ok(list);
    }

    [HttpPost("coaches/{id:int}/rating")]
    public async Task<IActionResult> RateCoach(int id, RateCoachRequest req)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        if (await _coaches.FindByIdAsync(id) is null)
            return NotFound(new MessageResponse("Không tìm thấy huấn luyện viên."));

        // Only coaches who have actually trained this member can be rated.
        var myCoachIds = await MyCoachIdsAsync(member.Id);
        if (!myCoachIds.Contains(id))
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn chỉ có thể đánh giá HLV của lớp bạn đã tham gia."));

        await _ratings.UpsertAsync(member.Id, id, req.Rating, req.Comment);
        return Ok(new MessageResponse("Đã gửi đánh giá."));
    }

    // Coach ids behind every class this member has ever enrolled in.
    private async Task<HashSet<int>> MyCoachIdsAsync(int memberId) =>
        (await _enrollments.FindByMemberIdAsync(memberId))
            .Where(e => e.Class?.CoachId != null)
            .Select(e => e.Class!.CoachId!.Value)
            .ToHashSet();

    // ── Module 6: Health tracking ────────────────────────────────────────────
    [HttpGet("health")]
    public async Task<IActionResult> MyHealth()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        return Ok((await _health.FindByMemberIdAsync(member.Id)).Select(HealthMetricDto.From));
    }

    [HttpPost("health")]
    public async Task<IActionResult> AddHealth(SaveHealthMetricRequest req)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        await _health.SaveAsync(new HealthMetric
        {
            MemberId = member.Id,
            RecordedDate = req.RecordedDate,
            WeightKg = req.WeightKg,
            HeightCm = req.HeightCm,
            BodyFatPct = req.BodyFatPct,
            Notes = req.Notes,
            CreatedAt = DateTime.Now,
        });
        return Ok(new MessageResponse("Đã lưu chỉ số sức khỏe."));
    }

    [HttpDelete("health/{id:int}")]
    public async Task<IActionResult> DeleteHealth(int id)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        var metric = await _health.FindByIdAsync(id);
        if (metric is null || metric.MemberId != member.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền với bản ghi này."));
        await _health.DeleteAsync(metric);
        return Ok(new MessageResponse("Đã xóa bản ghi."));
    }

    // ── Module 7: PT booking ─────────────────────────────────────────────────
    [HttpGet("pt-sessions")]
    public async Task<IActionResult> MyPtSessions()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        return Ok((await _pt.FindByMemberIdAsync(member.Id)).Select(PtSessionDto.From));
    }

    [HttpPost("pt-sessions")]
    public async Task<IActionResult> BookPt(BookPtRequest req)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var coach = await _coaches.FindByIdAsync(req.CoachId);
        if (coach is null || coach.Status != "ACTIVE")
            return BadRequest(new MessageResponse("Huấn luyện viên không khả dụng."));
        if (!TimeOnly.TryParse(req.StartTime, out var start) || !TimeOnly.TryParse(req.EndTime, out var end))
            return BadRequest(new MessageResponse("Giờ bắt đầu/kết thúc không hợp lệ."));
        if (end <= start)
            return BadRequest(new MessageResponse("Giờ kết thúc phải sau giờ bắt đầu."));

        await _pt.SaveAsync(new PtSession
        {
            MemberId = member.Id,
            CoachId = req.CoachId,
            SessionDate = req.SessionDate,
            StartTime = start,
            EndTime = end,
            Status = "PENDING",
            Notes = req.Notes,
            CreatedAt = DateTime.Now,
        });
        return Ok(new MessageResponse("Đã đặt lịch PT. Chờ huấn luyện viên xác nhận."));
    }

    [HttpPost("pt-sessions/{id:int}/cancel")]
    public async Task<IActionResult> CancelPt(int id)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        var s = await _pt.FindByIdAsync(id);
        if (s is null || s.MemberId != member.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền với lịch này."));
        if (s.Status is "CANCELLED" or "COMPLETED")
            return BadRequest(new MessageResponse("Lịch đã kết thúc hoặc đã hủy."));
        await _pt.UpdateStatusAsync(s, "CANCELLED");
        return Ok(new MessageResponse("Đã hủy lịch PT."));
    }
}
