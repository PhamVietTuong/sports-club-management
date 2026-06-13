using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the enrollments table.</summary>
public class EnrollmentRepository
{
    private readonly AppDbContext _db;
    public EnrollmentRepository(AppDbContext db) => _db = db;

    public Task<List<Enrollment>> FindByMemberIdAsync(int memberId) =>
        _db.Enrollments.Include(e => e.Class).Include(e => e.Member)
            .Where(e => e.MemberId == memberId)
            .OrderByDescending(e => e.EnrollDate).ToListAsync();

    public Task<List<Enrollment>> FindActiveByClassIdAsync(int classId) =>
        _db.Enrollments.Include(e => e.Class).Include(e => e.Member)
            .Where(e => e.ClassId == classId && e.Status == "ACTIVE").ToListAsync();

    public Task<bool> IsEnrolledAsync(int memberId, int classId) =>
        _db.Enrollments.AnyAsync(e =>
            e.MemberId == memberId && e.ClassId == classId && e.Status == "ACTIVE");

    /// <summary>
    /// Enroll the member, reactivating a previously cancelled row if present
    /// (the unique (member,class) constraint forbids a second insert).
    /// Returns true only if the member transitioned into ACTIVE (i.e. a seat
    /// should be counted); false if they were already ACTIVE.
    /// </summary>
    public async Task<bool> InsertAsync(int memberId, int classId)
    {
        var existing = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.MemberId == memberId && e.ClassId == classId);
        if (existing is not null)
        {
            if (existing.Status == "ACTIVE") return false; // already enrolled, no seat change
            existing.Status = "ACTIVE";
            existing.EnrollDate = DateOnly.FromDateTime(DateTime.Today);
        }
        else
        {
            _db.Enrollments.Add(new Enrollment
            {
                MemberId = memberId,
                ClassId = classId,
                EnrollDate = DateOnly.FromDateTime(DateTime.Today),
                Status = "ACTIVE",
            });
        }
        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Cancel an enrollment. Returns true only if an ACTIVE row was actually
    /// cancelled (i.e. a seat should be released); false otherwise.
    /// </summary>
    public async Task<bool> CancelAsync(int memberId, int classId)
    {
        var e = await _db.Enrollments
            .FirstOrDefaultAsync(x => x.MemberId == memberId && x.ClassId == classId);
        if (e is null || e.Status != "ACTIVE") return false;
        e.Status = "CANCELLED";
        await _db.SaveChangesAsync();
        return true;
    }
}
