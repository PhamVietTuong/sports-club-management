using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the attendance table.</summary>
public class AttendanceRepository
{
    private readonly AppDbContext _db;
    public AttendanceRepository(AppDbContext db) => _db = db;

    public Task<List<Attendance>> FindByClassAndDateAsync(int classId, DateOnly date) =>
        _db.Attendances.Include(a => a.Member)
            .Where(a => a.ClassId == classId && a.SessionDate == date)
            .ToListAsync();

    public Task<List<Attendance>> FindByMemberIdAsync(int memberId) =>
        _db.Attendances.Include(a => a.Class)
            .Where(a => a.MemberId == memberId)
            .OrderByDescending(a => a.SessionDate).ToListAsync();

    /// <summary>
    /// Insert or update one member's attendance for a (class, date). When the
    /// member self check-in, <paramref name="checkedIn"/> stamps the time; a coach
    /// marking the status leaves it null. Returns the persisted row.
    /// </summary>
    public async Task<Attendance> UpsertAsync(int classId, int memberId, DateOnly date,
        string status, bool checkedIn)
    {
        var row = await _db.Attendances.FirstOrDefaultAsync(a =>
            a.ClassId == classId && a.MemberId == memberId && a.SessionDate == date);
        if (row is null)
        {
            row = new Attendance
            {
                ClassId = classId,
                MemberId = memberId,
                SessionDate = date,
                Status = status,
                CheckedInAt = checkedIn ? DateTime.Now : null,
            };
            _db.Attendances.Add(row);
        }
        else
        {
            row.Status = status;
            if (checkedIn) row.CheckedInAt = DateTime.Now;
        }
        await _db.SaveChangesAsync();
        return row;
    }
}
