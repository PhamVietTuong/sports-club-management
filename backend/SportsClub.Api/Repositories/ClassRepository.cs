using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the training_classes table.</summary>
public class ClassRepository
{
    private readonly AppDbContext _db;
    public ClassRepository(AppDbContext db) => _db = db;

    public Task<List<TrainingClass>> FindAllAsync() =>
        _db.TrainingClasses.Include(c => c.Coach).OrderBy(c => c.Id).ToListAsync();

    /// <summary>One page of classes, filtered by a name/level search.</summary>
    public Task<PagedResult<TrainingClass>> FindPagedAsync(int page, int pageSize, string? search)
    {
        var q = _db.TrainingClasses.Include(c => c.Coach).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(c => c.Name.Contains(s) || (c.Level != null && c.Level.Contains(s)));
        }
        return q.OrderBy(c => c.Id).ToPagedResultAsync(page, pageSize);
    }

    public Task<List<TrainingClass>> FindActiveAsync() =>
        _db.TrainingClasses.Include(c => c.Coach).Where(c => c.IsActive)
            .OrderBy(c => c.Id).ToListAsync();

    public Task<List<TrainingClass>> FindByCoachIdAsync(int coachId) =>
        _db.TrainingClasses.Include(c => c.Coach).Where(c => c.CoachId == coachId)
            .OrderBy(c => c.Id).ToListAsync();

    public Task<TrainingClass?> FindByIdAsync(int id) =>
        _db.TrainingClasses.Include(c => c.Coach).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<int> InsertAsync(TrainingClass tc)
    {
        _db.TrainingClasses.Add(tc);
        await _db.SaveChangesAsync();
        return tc.Id;
    }

    public async Task UpdateAsync(TrainingClass tc)
    {
        _db.TrainingClasses.Update(tc);
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Atomically reserve one seat: a single conditional UPDATE that only
    /// increments when the class is active and has room. Returns true if a seat
    /// was reserved. This is race-free — two concurrent callers for the last
    /// seat cannot both succeed (the WHERE re-checks capacity inside the UPDATE).
    /// </summary>
    public async Task<bool> TryIncrementEnrolledAsync(int classId)
    {
        var affected = await _db.TrainingClasses
            .Where(c => c.Id == classId && c.IsActive && c.CurrentEnrolled < c.Capacity)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.CurrentEnrolled, c => c.CurrentEnrolled + 1));
        return affected > 0;
    }

    /// <summary>Atomically release one seat (never goes below zero).</summary>
    public Task DecrementEnrolledAsync(int classId) =>
        _db.TrainingClasses
            .Where(c => c.Id == classId && c.CurrentEnrolled > 0)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.CurrentEnrolled, c => c.CurrentEnrolled - 1));

    public Task<int> CountActiveAsync() =>
        _db.TrainingClasses.CountAsync(c => c.IsActive);

    /// <summary>Active classes with no coach assigned — available for a coach to claim.</summary>
    public Task<List<TrainingClass>> FindUnassignedActiveAsync() =>
        _db.TrainingClasses.Where(c => c.IsActive && c.CoachId == null)
            .OrderBy(c => c.Id).ToListAsync();

    /// <summary>
    /// Atomically claim an unassigned class: a conditional UPDATE that only sets
    /// the coach when the class is active and currently unassigned. Race-free —
    /// two coaches cannot both claim the same class.
    /// </summary>
    public async Task<bool> TryClaimAsync(int classId, int coachId)
    {
        var affected = await _db.TrainingClasses
            .Where(c => c.Id == classId && c.IsActive && c.CoachId == null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.CoachId, coachId));
        return affected > 0;
    }

    /// <summary>Release a class back to the unassigned pool — only the owning coach may.</summary>
    public async Task<bool> ReleaseAsync(int classId, int coachId)
    {
        var affected = await _db.TrainingClasses
            .Where(c => c.Id == classId && c.CoachId == coachId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.CoachId, (int?)null));
        return affected > 0;
    }
}
