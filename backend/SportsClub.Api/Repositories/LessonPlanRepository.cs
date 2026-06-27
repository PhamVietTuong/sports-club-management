using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the lesson_plans table.</summary>
public class LessonPlanRepository
{
    private readonly AppDbContext _db;
    public LessonPlanRepository(AppDbContext db) => _db = db;

    public Task<List<LessonPlan>> FindByCoachIdAsync(int coachId) =>
        _db.LessonPlans.Include(p => p.Class)
            .Where(p => p.CoachId == coachId)
            .OrderByDescending(p => p.CreatedAt).ToListAsync();

    public Task<List<LessonPlan>> FindByClassIdsAsync(IEnumerable<int> classIds) =>
        _db.LessonPlans.Include(p => p.Class)
            .Where(p => classIds.Contains(p.ClassId))
            .OrderByDescending(p => p.CreatedAt).ToListAsync();

    public Task<PagedResult<LessonPlan>> FindPagedByCoachAsync(int coachId, int page, int pageSize, string? search)
    {
        var q = _db.LessonPlans.Include(p => p.Class).Where(p => p.CoachId == coachId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(p => p.Title.Contains(s) || p.Class.Name.Contains(s));
        }
        return q.OrderByDescending(p => p.CreatedAt).ToPagedResultAsync(page, pageSize);
    }

    public Task<PagedResult<LessonPlan>> FindPagedByClassIdsAsync(
        IEnumerable<int> classIds, int page, int pageSize, string? search)
    {
        var q = _db.LessonPlans.Include(p => p.Class).Where(p => classIds.Contains(p.ClassId));
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(p => p.Title.Contains(s) || p.Class.Name.Contains(s));
        }
        return q.OrderByDescending(p => p.CreatedAt).ToPagedResultAsync(page, pageSize);
    }

    public Task<LessonPlan?> FindByIdAsync(int id) =>
        _db.LessonPlans.Include(p => p.Class).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> SaveAsync(LessonPlan plan)
    {
        _db.LessonPlans.Add(plan);
        await _db.SaveChangesAsync();
        return plan.Id;
    }

    public async Task DeleteAsync(LessonPlan plan)
    {
        _db.LessonPlans.Remove(plan);
        await _db.SaveChangesAsync();
    }
}
