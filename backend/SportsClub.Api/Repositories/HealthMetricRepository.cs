using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the health_metrics table.</summary>
public class HealthMetricRepository
{
    private readonly AppDbContext _db;
    public HealthMetricRepository(AppDbContext db) => _db = db;

    public Task<List<HealthMetric>> FindByMemberIdAsync(int memberId) =>
        _db.HealthMetrics.Where(h => h.MemberId == memberId)
            .OrderByDescending(h => h.RecordedDate).ThenByDescending(h => h.Id)
            .ToListAsync();

    public Task<PagedResult<HealthMetric>> FindPagedByMemberAsync(int memberId, int page, int pageSize, string? search)
    {
        var q = _db.HealthMetrics.Where(h => h.MemberId == memberId);
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(h => h.Notes != null && h.Notes.Contains(search.Trim()));
        return q.OrderByDescending(h => h.RecordedDate).ThenByDescending(h => h.Id)
            .ToPagedResultAsync(page, pageSize);
    }

    public Task<HealthMetric?> FindByIdAsync(int id) =>
        _db.HealthMetrics.FirstOrDefaultAsync(h => h.Id == id);

    public async Task<int> SaveAsync(HealthMetric metric)
    {
        _db.HealthMetrics.Add(metric);
        await _db.SaveChangesAsync();
        return metric.Id;
    }

    public async Task DeleteAsync(HealthMetric metric)
    {
        _db.HealthMetrics.Remove(metric);
        await _db.SaveChangesAsync();
    }
}
