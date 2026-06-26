using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the pt_sessions table.</summary>
public class PtSessionRepository
{
    private readonly AppDbContext _db;
    public PtSessionRepository(AppDbContext db) => _db = db;

    public Task<List<PtSession>> FindByMemberIdAsync(int memberId) =>
        _db.PtSessions.Include(s => s.Coach).Include(s => s.Member)
            .Where(s => s.MemberId == memberId)
            .OrderByDescending(s => s.SessionDate).ThenByDescending(s => s.StartTime)
            .ToListAsync();

    public Task<List<PtSession>> FindByCoachIdAsync(int coachId) =>
        _db.PtSessions.Include(s => s.Coach).Include(s => s.Member)
            .Where(s => s.CoachId == coachId)
            .OrderByDescending(s => s.SessionDate).ThenByDescending(s => s.StartTime)
            .ToListAsync();

    public Task<PtSession?> FindByIdAsync(int id) =>
        _db.PtSessions.Include(s => s.Coach).Include(s => s.Member)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<int> SaveAsync(PtSession session)
    {
        _db.PtSessions.Add(session);
        await _db.SaveChangesAsync();
        return session.Id;
    }

    public async Task UpdateStatusAsync(PtSession session, string status)
    {
        session.Status = status;
        await _db.SaveChangesAsync();
    }
}
