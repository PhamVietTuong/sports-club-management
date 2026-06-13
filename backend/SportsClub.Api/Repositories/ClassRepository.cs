using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the training_classes table.</summary>
public class ClassRepository
{
    private readonly AppDbContext _db;
    public ClassRepository(AppDbContext db) => _db = db;

    public Task<List<TrainingClass>> FindAllAsync() =>
        _db.TrainingClasses.Include(c => c.Coach).OrderBy(c => c.Id).ToListAsync();

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
}
