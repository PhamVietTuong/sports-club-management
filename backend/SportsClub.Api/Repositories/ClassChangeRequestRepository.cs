using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the class_change_requests table
/// (coach claim/release requests awaiting admin approval).</summary>
public class ClassChangeRequestRepository
{
    private readonly AppDbContext _db;
    public ClassChangeRequestRepository(AppDbContext db) => _db = db;

    public Task<List<ClassChangeRequest>> FindAllAsync(string? status) =>
        _db.ClassChangeRequests
            .Include(r => r.Coach)
            .Include(r => r.Class)
            .Where(r => status == null || r.Status == status)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    public Task<List<ClassChangeRequest>> FindByCoachIdAsync(int coachId) =>
        _db.ClassChangeRequests.Include(r => r.Class)
            .Where(r => r.CoachId == coachId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    public Task<ClassChangeRequest?> FindByIdAsync(int id) =>
        _db.ClassChangeRequests
            .Include(r => r.Coach)
            .Include(r => r.Class)
            .FirstOrDefaultAsync(r => r.Id == id);

    /// <summary>True if a class already has an unresolved (PENDING) request — guards
    /// against duplicate/conflicting claim/release requests on the same class.</summary>
    public Task<bool> HasPendingForClassAsync(int classId) =>
        _db.ClassChangeRequests.AnyAsync(r => r.ClassId == classId && r.Status == "PENDING");

    public async Task<int> SaveAsync(ClassChangeRequest r)
    {
        _db.ClassChangeRequests.Add(r);
        await _db.SaveChangesAsync();
        return r.Id;
    }

    public async Task UpdateAsync(ClassChangeRequest r)
    {
        _db.ClassChangeRequests.Update(r);
        await _db.SaveChangesAsync();
    }
}
