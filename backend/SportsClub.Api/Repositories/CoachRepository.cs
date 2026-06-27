using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the coaches table.</summary>
public class CoachRepository
{
    private readonly AppDbContext _db;
    public CoachRepository(AppDbContext db) => _db = db;

    public Task<List<Coach>> FindAllAsync() =>
        _db.Coaches.Include(c => c.User).OrderBy(c => c.Id).ToListAsync();

    /// <summary>One page of coaches, filtered by status and a free-text search
    /// over name / username / email / specialization.</summary>
    public Task<PagedResult<Coach>> FindPagedAsync(int page, int pageSize, string? search, string? status)
    {
        var q = _db.Coaches.Include(c => c.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(c => c.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(c => c.FullName.Contains(s)
                             || c.User.Username.Contains(s)
                             || c.User.Email.Contains(s)
                             || (c.Specialization != null && c.Specialization.Contains(s)));
        }
        return q.OrderBy(c => c.Id).ToPagedResultAsync(page, pageSize);
    }

    public Task<List<Coach>> FindByStatusAsync(string status) =>
        _db.Coaches.Include(c => c.User)
            .Where(c => c.Status == status)
            .OrderBy(c => c.Id).ToListAsync();

    public Task<List<Coach>> FindActiveAsync() =>
        _db.Coaches.Include(c => c.User)
            .Where(c => c.Status == "ACTIVE")
            .OrderBy(c => c.FullName).ToListAsync();

    /// <summary>One page of ACTIVE coaches, filtered by name / specialization —
    /// for the member's "rate a coach" list.</summary>
    public Task<PagedResult<Coach>> FindPagedActiveAsync(int page, int pageSize, string? search)
    {
        var q = _db.Coaches.Include(c => c.User).Where(c => c.Status == "ACTIVE");
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(c => c.FullName.Contains(s) || (c.Specialization != null && c.Specialization.Contains(s)));
        }
        return q.OrderBy(c => c.FullName).ToPagedResultAsync(page, pageSize);
    }

    public Task<Coach?> FindByIdAsync(int id) =>
        _db.Coaches.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);

    public Task<Coach?> FindByUserIdAsync(int userId) =>
        _db.Coaches.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task<int> InsertAsync(int userId, string fullName, string? specialization,
        string? bio, int experience, decimal salary)
    {
        var coach = new Coach
        {
            UserId = userId,
            FullName = fullName,
            Specialization = specialization,
            Bio = bio,
            Experience = experience,
            Salary = salary,
            Status = "ACTIVE",
        };
        _db.Coaches.Add(coach);
        await _db.SaveChangesAsync();
        return coach.Id;
    }

    public async Task UpdateAsync(Coach c)
    {
        _db.Coaches.Update(c);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        var coach = await _db.Coaches.FirstOrDefaultAsync(c => c.Id == id);
        if (coach is null) return;
        coach.Status = status;
        await _db.SaveChangesAsync();
    }

    public Task<int> CountAllAsync() => _db.Coaches.CountAsync();
}
