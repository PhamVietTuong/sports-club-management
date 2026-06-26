using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the coach_ratings table.</summary>
public class CoachRatingRepository
{
    private readonly AppDbContext _db;
    public CoachRatingRepository(AppDbContext db) => _db = db;

    public Task<List<CoachRating>> FindByCoachIdAsync(int coachId) =>
        _db.CoachRatings.Include(r => r.Member)
            .Where(r => r.CoachId == coachId)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();

    public Task<List<CoachRating>> FindByMemberIdAsync(int memberId) =>
        _db.CoachRatings.Where(r => r.MemberId == memberId).ToListAsync();

    /// <summary>Per-coach average rating + count, computed in the database.</summary>
    public async Task<List<CoachRatingAggregate>> AveragesAsync()
    {
        var rows = await _db.CoachRatings
            .GroupBy(r => r.CoachId)
            .Select(g => new { CoachId = g.Key, Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
            .ToListAsync();
        return rows.Select(r => new CoachRatingAggregate(r.CoachId, r.Avg, r.Count)).ToList();
    }

    /// <summary>Insert or update the member's single rating for a coach.</summary>
    public async Task UpsertAsync(int memberId, int coachId, int rating, string? comment)
    {
        var row = await _db.CoachRatings
            .FirstOrDefaultAsync(r => r.MemberId == memberId && r.CoachId == coachId);
        if (row is null)
        {
            _db.CoachRatings.Add(new CoachRating
            {
                MemberId = memberId,
                CoachId = coachId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.Now,
            });
        }
        else
        {
            row.Rating = rating;
            row.Comment = comment;
            row.CreatedAt = DateTime.Now;
        }
        await _db.SaveChangesAsync();
    }
}

/// <summary>Aggregated rating stats for one coach.</summary>
public record CoachRatingAggregate(int CoachId, double Average, int Count);
