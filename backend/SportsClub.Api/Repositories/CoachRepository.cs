using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the coaches table.</summary>
public class CoachRepository
{
    private readonly AppDbContext _db;
    public CoachRepository(AppDbContext db) => _db = db;

    public Task<List<Coach>> FindAllAsync() =>
        _db.Coaches.Include(c => c.User).OrderBy(c => c.Id).ToListAsync();

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

    public Task<int> CountAllAsync() => _db.Coaches.CountAsync();
}
