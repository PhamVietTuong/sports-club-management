using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the schedules table.</summary>
public class ScheduleRepository
{
    private readonly AppDbContext _db;
    public ScheduleRepository(AppDbContext db) => _db = db;

    public Task<List<Schedule>> FindAllAsync() =>
        _db.Schedules.Include(s => s.Class).OrderBy(s => s.Id).ToListAsync();

    public Task<List<Schedule>> FindByClassIdAsync(int classId) =>
        _db.Schedules.Include(s => s.Class).Where(s => s.ClassId == classId).ToListAsync();

    public Task<List<Schedule>> FindByCoachIdAsync(int coachId) =>
        _db.Schedules.Include(s => s.Class)
            .Where(s => s.Class.CoachId == coachId).ToListAsync();

    public Task<List<Schedule>> FindByMemberIdAsync(int memberId) =>
        (from s in _db.Schedules.Include(s => s.Class)
         join e in _db.Enrollments on s.ClassId equals e.ClassId
         where e.MemberId == memberId && e.Status == "ACTIVE"
         select s).ToListAsync();

    public Task<Schedule?> FindByIdAsync(int id) =>
        _db.Schedules.Include(s => s.Class).FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> SaveAsync(Schedule s)
    {
        _db.Schedules.Add(s);
        await _db.SaveChangesAsync();
        return s.Id;
    }

    public async Task UpdateAsync(Schedule s)
    {
        _db.Schedules.Update(s);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var s = await _db.Schedules.FirstOrDefaultAsync(x => x.Id == id);
        if (s is null) return;
        _db.Schedules.Remove(s);
        await _db.SaveChangesAsync();
    }
}
