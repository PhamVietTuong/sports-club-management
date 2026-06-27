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
[Route("api/coach")]
[Authorize(Roles = UserRole.Coach)]
public class CoachController : ControllerBase
{
    private readonly CoachRepository _coaches;
    private readonly ClassRepository _classes;
    private readonly ScheduleRepository _schedules;
    private readonly EnrollmentRepository _enrollments;
    private readonly AttendanceRepository _attendance;
    private readonly LessonPlanRepository _lessonPlans;
    private readonly ProgressNoteRepository _progress;
    private readonly CoachRatingRepository _ratings;
    private readonly PtSessionRepository _pt;
    private readonly ClassChangeRequestRepository _classRequests;

    public CoachController(CoachRepository coaches, ClassRepository classes,
        ScheduleRepository schedules, EnrollmentRepository enrollments,
        AttendanceRepository attendance, LessonPlanRepository lessonPlans,
        ProgressNoteRepository progress, CoachRatingRepository ratings,
        PtSessionRepository pt, ClassChangeRequestRepository classRequests)
    {
        _coaches = coaches;
        _classes = classes;
        _schedules = schedules;
        _enrollments = enrollments;
        _attendance = attendance;
        _lessonPlans = lessonPlans;
        _progress = progress;
        _ratings = ratings;
        _pt = pt;
        _classRequests = classRequests;
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
        var schedules = await _schedules.FindByClassIdAsync(id);
        return Ok(new
        {
            Class = ClassDto.From(selected),
            EnrolledMembers = enrolled.Select(EnrollmentDto.From),
            Schedules = schedules.Select(ScheduleDto.From),
        });
    }

    // ── Module 3: Attendance (coach marks per session) ───────────────────────
    /// <summary>
    /// Roster for one class on one date: every actively-enrolled member plus
    /// their mark for that day (null = not yet marked). IDOR-guarded.
    /// </summary>
    [HttpGet("classes/{id:int}/attendance")]
    public async Task<IActionResult> AttendanceRoster(int id, [FromQuery] DateOnly? date)
    {
        var (_, cls, error) = await ResolveOwnedClass(id);
        if (error is not null) return error;

        var day = date ?? DateOnly.FromDateTime(DateTime.Today);
        var enrolled = await _enrollments.FindActiveByClassIdAsync(id);
        var marks = (await _attendance.FindByClassAndDateAsync(id, day))
            .ToDictionary(a => a.MemberId);

        var roster = enrolled.Select(e =>
        {
            marks.TryGetValue(e.MemberId, out var a);
            return new AttendanceRosterEntryDto(
                e.MemberId, e.Member?.FullName ?? "", a?.Status, a?.CheckedInAt);
        });
        var schedules = await _schedules.FindByClassIdAsync(id);
        return Ok(new
        {
            Date = day,
            Class = ClassDto.From(cls!),
            Schedules = schedules.Select(ScheduleDto.From),
            Roster = roster,
        });
    }

    [HttpPost("classes/{id:int}/attendance")]
    public async Task<IActionResult> MarkAttendance(int id, MarkAttendanceRequest req)
    {
        var (_, _, error) = await ResolveOwnedClass(id);
        if (error is not null) return error;

        if (!AttendanceDto.AllowedStatuses.Contains(req.Status))
            return BadRequest(new MessageResponse("Trạng thái điểm danh không hợp lệ."));
        if (!await _enrollments.IsEnrolledAsync(req.MemberId, id))
            return BadRequest(new MessageResponse("Thành viên chưa đăng ký lớp này."));

        await _attendance.UpsertAsync(id, req.MemberId, req.SessionDate, req.Status, checkedIn: false);
        return Ok(new MessageResponse("Đã điểm danh."));
    }

    // ── Module 4: Lesson plans + progress notes ──────────────────────────────
    [HttpGet("lesson-plans")]
    public async Task<IActionResult> MyLessonPlans()
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));
        return Ok((await _lessonPlans.FindByCoachIdAsync(coach.Id)).Select(LessonPlanDto.From));
    }

    [HttpPost("lesson-plans")]
    public async Task<IActionResult> CreateLessonPlan(SaveLessonPlanRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title))
            return BadRequest(new MessageResponse("Tiêu đề là bắt buộc."));
        var (coach, _, error) = await ResolveOwnedClass(req.ClassId);
        if (error is not null) return error;

        await _lessonPlans.SaveAsync(new LessonPlan
        {
            ClassId = req.ClassId,
            CoachId = coach!.Id,
            Title = req.Title.Trim(),
            Content = req.Content,
            CreatedAt = DateTime.Now,
        });
        return Ok(new MessageResponse("Đã tạo giáo án."));
    }

    [HttpDelete("lesson-plans/{id:int}")]
    public async Task<IActionResult> DeleteLessonPlan(int id)
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));
        var plan = await _lessonPlans.FindByIdAsync(id);
        if (plan is null || plan.CoachId != coach.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền với giáo án này."));
        await _lessonPlans.DeleteAsync(plan);
        return Ok(new MessageResponse("Đã xóa giáo án."));
    }

    [HttpGet("progress")]
    public async Task<IActionResult> MyProgressNotes()
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));
        return Ok((await _progress.FindByCoachIdAsync(coach.Id)).Select(ProgressNoteDto.From));
    }

    [HttpPost("progress")]
    public async Task<IActionResult> CreateProgressNote(SaveProgressNoteRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Note))
            return BadRequest(new MessageResponse("Nội dung là bắt buộc."));
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));

        // AUTHORIZATION — the member must be actively enrolled in one of THIS
        // coach's classes, so a coach cannot write notes about arbitrary members.
        var myClassIds = (await _classes.FindByCoachIdAsync(coach.Id)).Select(c => c.Id).ToHashSet();
        var memberActiveClassIds = (await _enrollments.FindByMemberIdAsync(req.MemberId))
            .Where(e => e.Status == "ACTIVE").Select(e => e.ClassId).ToHashSet();
        if (!memberActiveClassIds.Overlaps(myClassIds))
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Học viên không thuộc lớp của bạn."));

        // If a class is named, it must be one of the coach's own classes.
        if (req.ClassId is { } cid && !myClassIds.Contains(cid))
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền với lớp học này."));

        await _progress.SaveAsync(new ProgressNote
        {
            MemberId = req.MemberId,
            CoachId = coach.Id,
            ClassId = req.ClassId,
            Note = req.Note.Trim(),
            Rating = req.Rating,
            RecordedAt = DateTime.Now,
        });
        return Ok(new MessageResponse("Đã lưu đánh giá tiến độ."));
    }

    // ── Module 5: Ratings about me ───────────────────────────────────────────
    [HttpGet("ratings")]
    public async Task<IActionResult> MyRatings()
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));
        var ratings = await _ratings.FindByCoachIdAsync(coach.Id);
        var avg = ratings.Count == 0 ? 0 : Math.Round(ratings.Average(r => (double)r.Rating), 1);
        return Ok(new CoachRatingSummaryDto(avg, ratings.Count, ratings.Select(CoachRatingDto.From)));
    }

    // ── Module 7: PT sessions booked with me ─────────────────────────────────
    [HttpGet("pt-sessions")]
    public async Task<IActionResult> MyPtSessions()
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));
        return Ok((await _pt.FindByCoachIdAsync(coach.Id)).Select(PtSessionDto.From));
    }

    [HttpPost("pt-sessions/{id:int}/status")]
    public async Task<IActionResult> UpdatePtStatus(int id, UpdatePtStatusRequest req)
    {
        if (!UpdatePtStatusRequest.Allowed.Contains(req.Status))
            return BadRequest(new MessageResponse("Trạng thái không hợp lệ."));
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));
        var s = await _pt.FindByIdAsync(id);
        if (s is null || s.CoachId != coach.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền với lịch này."));

        // SCHEDULE CLASH GUARD — re-check when confirming, since the coach may have
        // taken on a class since the member booked this PT slot.
        if (req.Status == "CONFIRMED")
        {
            var classClash = ScheduleClash.FindClassClash(
                s.SessionDate, s.StartTime, s.EndTime, await _schedules.FindByCoachIdAsync(coach.Id));
            if (classClash is not null)
                return BadRequest(new MessageResponse(
                    $"Không thể xác nhận — trùng lớp \"{classClash.Class?.Name}\" " +
                    $"({classClash.StartTime:HH\\:mm}–{classClash.EndTime:HH\\:mm}) cùng khung giờ."));
        }

        await _pt.UpdateStatusAsync(s, req.Status);
        return Ok(new MessageResponse("Đã cập nhật trạng thái lịch PT."));
    }

    // ── Module 8: Nhận lớp (claim / release an unassigned class) ─────────────
    [HttpGet("available-classes")]
    public async Task<IActionResult> AvailableClasses()
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));

        var classes = await _classes.FindUnassignedActiveAsync();
        var classIds = classes.Select(c => c.Id).ToList();

        // Attach each class's weekly schedule (so the coach sees when it runs and
        // can judge clashes) and the members already enrolled (so they know who
        // they'd be teaching) before requesting to claim it.
        var schedulesByClass = (await _schedules.FindByClassIdsAsync(classIds))
            .GroupBy(s => s.ClassId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var membersByClass = (await _enrollments.FindActiveByClassIdsAsync(classIds))
            .GroupBy(e => e.ClassId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return Ok(classes.Select(c => new
        {
            Class = ClassDto.From(c),
            Schedules = (schedulesByClass.GetValueOrDefault(c.Id) ?? new List<Schedule>())
                .Select(ScheduleDto.From),
            EnrolledMembers = (membersByClass.GetValueOrDefault(c.Id) ?? new List<Enrollment>())
                .Select(EnrollmentDto.From),
        }));
    }

    [HttpGet("class-requests")]
    public async Task<IActionResult> MyClassRequests()
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));
        return Ok((await _classRequests.FindByCoachIdAsync(coach.Id)).Select(ClassChangeRequestDto.From));
    }

    /// <summary>
    /// Request to accept (claim) an unassigned class. This no longer assigns the
    /// class directly — it posts a PENDING request for admin approval. The
    /// coach's timetable is checked first so a clashing class is rejected early.
    /// </summary>
    [HttpPost("classes/{id:int}/claim")]
    public async Task<IActionResult> ClaimClass(int id)
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));

        var cls = await _classes.FindByIdAsync(id);
        if (cls is null || !cls.IsActive)
            return BadRequest(new MessageResponse("Lớp không khả dụng."));
        if (cls.CoachId != null)
            return BadRequest(new MessageResponse("Lớp đã có huấn luyện viên."));
        if (await _classRequests.HasPendingForClassAsync(id))
            return BadRequest(new MessageResponse("Lớp này đang có yêu cầu chờ duyệt."));

        // SCHEDULE CLASH GUARD — the new class must not overlap the coach's
        // existing timetable on the same day/time.
        var conflict = ScheduleClash.FindConflict(
            await _schedules.FindByClassIdAsync(id),
            await _schedules.FindByCoachIdAsync(coach.Id));
        if (conflict is { } c)
            return BadRequest(new MessageResponse(
                $"Lịch lớp này trùng với lớp \"{c.existing.Class?.Name}\" " +
                $"({c.incoming.DayOfWeek} {c.incoming.StartTime:HH\\:mm}–{c.incoming.EndTime:HH\\:mm})."));

        await _classRequests.SaveAsync(new ClassChangeRequest
        {
            CoachId = coach.Id,
            ClassId = id,
            Action = "CLAIM",
            Status = "PENDING",
            RequestedAt = DateTime.Now,
        });
        return Ok(new MessageResponse("Đã gửi yêu cầu nhận lớp. Chờ quản trị viên duyệt."));
    }

    /// <summary>
    /// Request to give up (release) a class the coach owns. Posts a PENDING
    /// request for admin approval; the class stays assigned until approved.
    /// </summary>
    [HttpPost("classes/{id:int}/release")]
    public async Task<IActionResult> ReleaseClass(int id)
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null) return NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên."));

        var cls = await _classes.FindByIdAsync(id);
        if (cls is null || cls.CoachId != coach.Id)
            return StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không phụ trách lớp này."));
        if (await _classRequests.HasPendingForClassAsync(id))
            return BadRequest(new MessageResponse("Lớp này đang có yêu cầu chờ duyệt."));

        await _classRequests.SaveAsync(new ClassChangeRequest
        {
            CoachId = coach.Id,
            ClassId = id,
            Action = "RELEASE",
            Status = "PENDING",
            RequestedAt = DateTime.Now,
        });
        return Ok(new MessageResponse("Đã gửi yêu cầu trả lớp. Chờ quản trị viên duyệt."));
    }

    /// <summary>
    /// IDOR GUARD — resolve the logged-in coach and a class they own. Returns an
    /// error result (404/403) when the coach has no profile or the class is not theirs.
    /// </summary>
    private async Task<(Coach? coach, TrainingClass? cls, IActionResult? error)> ResolveOwnedClass(int classId)
    {
        var coach = await _coaches.FindByUserIdAsync(User.GetUserId());
        if (coach is null)
            return (null, null, NotFound(new MessageResponse("Không tìm thấy hồ sơ huấn luyện viên.")));
        var cls = await _classes.FindByIdAsync(classId);
        if (cls is null || cls.CoachId != coach.Id)
            return (coach, null, StatusCode(StatusCodes.Status403Forbidden,
                new MessageResponse("Bạn không có quyền với lớp học này.")));
        return (coach, cls, null);
    }
}
