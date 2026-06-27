using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;
using SportsClub.Api.Security;
using SportsClub.Api.Services;

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
    private readonly MembershipRequestRepository _membershipRequests;
    private readonly PackageClassRepository _packageClasses;

    public MemberController(MemberRepository members, UserRepository users,
        ClassRepository classes, ScheduleRepository schedules,
        EnrollmentRepository enrollments, PackageRepository packages,
        PaymentRepository payments, AttendanceRepository attendance,
        LessonPlanRepository lessonPlans, ProgressNoteRepository progress,
        CoachRepository coaches, CoachRatingRepository ratings,
        HealthMetricRepository health, PtSessionRepository pt,
        MembershipRequestRepository membershipRequests, PackageClassRepository packageClasses)
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
        _membershipRequests = membershipRequests;
        _packageClasses = packageClasses;
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

        // Only the classes attached to the member's current package are visible:
        // their active membership, or — if not yet activated — their approved one.
        var effective = await _membershipRequests.FindEffectiveAsync(member.Id);
        if (effective is null) return Ok(Array.Empty<object>());

        var myActive = (await _enrollments.FindByMemberIdAsync(member.Id))
            .Where(e => e.Status == "ACTIVE").Select(e => e.ClassId).ToHashSet();

        var packageClasses = await _packageClasses.FindClassesAsync(effective.PackageId);

        // Group each class's weekly schedule slots so the member sees when it runs.
        var schedulesByClass = (await _schedules.FindByClassIdsAsync(packageClasses.Select(c => c.Id)))
            .GroupBy(s => s.ClassId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // ITERATOR PATTERN — traverse the package's classes via the club iterator
        var classes = ClubCollection<TrainingClass>.Of(packageClasses);

        return Ok(classes.Select(c => new
        {
            Class = ClassDto.From(c),
            IsEnrolled = myActive.Contains(c.Id),
            Schedules = (schedulesByClass.GetValueOrDefault(c.Id) ?? new List<Schedule>())
                .Select(ScheduleDto.From),
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

        // Must hold a package (active or approved) that grants this class.
        var effective = await _membershipRequests.FindEffectiveAsync(member.Id);
        if (effective is null)
            return BadRequest(new MessageResponse("Bạn cần có gói tập được duyệt trước khi đăng ký lớp."));
        if (!await _packageClasses.IsLinkedAsync(effective.PackageId, id))
            return BadRequest(new MessageResponse("Lớp này không thuộc gói tập của bạn."));

        if (await _enrollments.IsEnrolledAsync(member.Id, id))
            return BadRequest(new MessageResponse("Bạn đã đăng ký rồi."));

        // Enforce the package's class quota (0 = no limit).
        var pkg = effective.Package ?? await _packages.FindByIdAsync(effective.PackageId);
        if (pkg is { MaxClasses: > 0 })
        {
            var activeCount = (await _enrollments.FindByMemberIdAsync(member.Id))
                .Count(e => e.Status == "ACTIVE");
            if (activeCount >= pkg.MaxClasses)
                return BadRequest(new MessageResponse($"Gói của bạn chỉ cho phép tối đa {pkg.MaxClasses} lớp."));
        }

        // The first class registration activates the membership and locks any
        // further cancellation/change of the package.
        if (effective.Status == "APPROVED")
            await ActivateMembershipAsync(member, effective);

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

    // ── Membership requests (register a package → admin approves → activate) ──
    [HttpGet("membership/requests")]
    public async Task<IActionResult> MyMembershipRequests()
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));
        return Ok((await _membershipRequests.FindByMemberIdAsync(member.Id)).Select(MembershipRequestDto.From));
    }

    /// <summary>Submit a request to register a package. Creates a PENDING request
    /// for the admin to approve — no charge happens yet.</summary>
    [HttpPost("membership/request")]
    public async Task<IActionResult> RequestMembership(BuyMembershipRequest req)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var method = string.IsNullOrEmpty(req.Method) ? "CASH" : req.Method;
        if (!BuyMembershipRequest.AllowedMethods.Contains(method))
            return BadRequest(new MessageResponse("Phương thức thanh toán không hợp lệ."));

        var pkg = await _packages.FindByIdAsync(req.PackageId);
        if (pkg is null || !pkg.IsActive)
            return BadRequest(new MessageResponse("Gói tập không khả dụng."));

        if (await _membershipRequests.HasOpenRequestAsync(member.Id))
            return BadRequest(new MessageResponse(
                "Bạn đang có một yêu cầu chờ xử lý. Vui lòng hoàn tất hoặc hủy yêu cầu đó trước."));

        await _membershipRequests.SaveAsync(new MembershipRequest
        {
            MemberId = member.Id,
            PackageId = pkg.Id,
            Amount = pkg.Price,
            Method = method,
            Status = "PENDING",
            RequestedAt = DateTime.Now,
        });

        return Ok(new MessageResponse("Đã gửi yêu cầu đăng ký gói tập. Vui lòng chờ quản trị viên duyệt."));
    }

    /// <summary>Cancel a request — allowed while PENDING, or while APPROVED inside
    /// the 24h grace window (before activation).</summary>
    [HttpPost("membership/requests/{id:int}/cancel")]
    public async Task<IActionResult> CancelMembershipRequest(int id)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var r = await _membershipRequests.FindByIdAsync(id);
        if (r is null || r.MemberId != member.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền với yêu cầu này."));
        if (!r.IsModifiable())
            return BadRequest(new MessageResponse("Không thể hủy yêu cầu này (đã kích hoạt hoặc quá hạn)."));

        r.Status = "CANCELLED";
        await _membershipRequests.UpdateAsync(r);
        return Ok(new MessageResponse("Đã hủy yêu cầu."));
    }

    /// <summary>Change the package on a modifiable request: the old one is
    /// cancelled and a new PENDING request is created for re-approval.</summary>
    [HttpPost("membership/requests/{id:int}/change")]
    public async Task<IActionResult> ChangeMembershipRequest(int id, ChangePackageRequest req)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var r = await _membershipRequests.FindByIdAsync(id);
        if (r is null || r.MemberId != member.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền với yêu cầu này."));
        if (!r.IsModifiable())
            return BadRequest(new MessageResponse("Không thể thay đổi yêu cầu này (đã kích hoạt hoặc quá hạn)."));

        var pkg = await _packages.FindByIdAsync(req.PackageId);
        if (pkg is null || !pkg.IsActive)
            return BadRequest(new MessageResponse("Gói tập không khả dụng."));

        r.Status = "CANCELLED";
        await _membershipRequests.UpdateAsync(r);

        await _membershipRequests.SaveAsync(new MembershipRequest
        {
            MemberId = member.Id,
            PackageId = pkg.Id,
            Amount = pkg.Price,
            Method = r.Method,
            Status = "PENDING",
            RequestedAt = DateTime.Now,
        });

        return Ok(new MessageResponse("Đã đổi gói tập. Yêu cầu mới đang chờ quản trị viên duyệt."));
    }

    /// <summary>Activate an approved membership: charges the package, extends the
    /// membership and locks the request (no further cancel/change).</summary>
    [HttpPost("membership/requests/{id:int}/activate")]
    public async Task<IActionResult> ActivateMembershipRequest(int id)
    {
        var member = await _members.FindByUserIdAsync(User.GetUserId());
        if (member is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ thành viên."));

        var r = await _membershipRequests.FindByIdAsync(id);
        if (r is null || r.MemberId != member.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền với yêu cầu này."));
        if (r.Status != "APPROVED")
            return BadRequest(new MessageResponse("Chỉ có thể kích hoạt gói tập đã được duyệt."));

        await ActivateMembershipAsync(member, r);
        return Ok(new MessageResponse($"Đã kích hoạt gói tập. Hạn mới: {member.ExpiryDate:dd/MM/yyyy}."));
    }

    /// <summary>
    /// Apply an approved membership: record the payment, extend the member's
    /// expiry (stacking onto remaining time), and mark the request ACTIVE. Shared
    /// by the explicit activate endpoint and the first-class-registration path.
    /// </summary>
    private async Task ActivateMembershipAsync(Member member, MembershipRequest r)
    {
        var pkg = r.Package ?? await _packages.FindByIdAsync(r.PackageId);

        await _payments.SaveAsync(new Payment
        {
            MemberId = member.Id,
            PackageId = r.PackageId,
            Amount = r.Amount,
            Method = r.Method,
            Status = "COMPLETED",
            Description = $"Kích hoạt gói {pkg?.Name}",
            PaidAt = DateTime.Now,
        });

        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = member.ExpiryDate is { } exp && exp > today ? exp : today;
        member.PackageId = r.PackageId;
        member.ExpiryDate = start.AddMonths(pkg?.DurationMonths ?? 0);
        member.Status = "ACTIVE";
        await _members.UpdateAsync(member);

        r.Status = "ACTIVE";
        r.StartDate = today;
        r.ActivatedAt = DateTime.Now;
        await _membershipRequests.UpdateAsync(r);
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

        // SCHEDULE CLASH GUARD — the PT slot must not overlap the coach's fixed
        // teaching timetable on that weekday.
        var classClash = ScheduleClash.FindClassClash(
            req.SessionDate, start, end, await _schedules.FindByCoachIdAsync(req.CoachId));
        if (classClash is not null)
            return BadRequest(new MessageResponse(
                $"HLV đang có lớp \"{classClash.Class?.Name}\" vào khung giờ này " +
                $"({classClash.StartTime:HH\\:mm}–{classClash.EndTime:HH\\:mm}). Vui lòng chọn giờ khác."));

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
