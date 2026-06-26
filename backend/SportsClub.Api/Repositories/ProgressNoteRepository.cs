using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the progress_notes table.</summary>
public class ProgressNoteRepository
{
    private readonly AppDbContext _db;
    public ProgressNoteRepository(AppDbContext db) => _db = db;

    public Task<List<ProgressNote>> FindByMemberIdAsync(int memberId) =>
        _db.ProgressNotes.Include(n => n.Member)
            .Where(n => n.MemberId == memberId)
            .OrderByDescending(n => n.RecordedAt).ToListAsync();

    public Task<List<ProgressNote>> FindByCoachIdAsync(int coachId) =>
        _db.ProgressNotes.Include(n => n.Member)
            .Where(n => n.CoachId == coachId)
            .OrderByDescending(n => n.RecordedAt).ToListAsync();

    public async Task<int> SaveAsync(ProgressNote note)
    {
        _db.ProgressNotes.Add(note);
        await _db.SaveChangesAsync();
        return note.Id;
    }
}
