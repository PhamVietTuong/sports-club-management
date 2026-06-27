using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the membership_requests table.</summary>
public class MembershipRequestRepository
{
    private readonly AppDbContext _db;
    public MembershipRequestRepository(AppDbContext db) => _db = db;

    /// <summary>One page of requests, filtered by status and a member-name /
    /// package-name search.</summary>
    public Task<PagedResult<MembershipRequest>> FindPagedAsync(
        int page, int pageSize, string? search, string? status)
    {
        var q = _db.MembershipRequests
            .Include(r => r.Member).ThenInclude(m => m.User)
            .Include(r => r.Package)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(r => r.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(r => r.Member.FullName.Contains(s) || r.Package.Name.Contains(s));
        }
        return q.OrderByDescending(r => r.RequestedAt).ToPagedResultAsync(page, pageSize);
    }

    /// <summary>All requests (optionally filtered by status), newest first — admin view.</summary>
    public Task<List<MembershipRequest>> FindAllAsync(string? status) =>
        _db.MembershipRequests
            .Include(r => r.Member).ThenInclude(m => m.User)
            .Include(r => r.Package)
            .Where(r => status == null || r.Status == status)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    public Task<List<MembershipRequest>> FindByMemberIdAsync(int memberId) =>
        _db.MembershipRequests.Include(r => r.Package)
            .Where(r => r.MemberId == memberId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

    public Task<MembershipRequest?> FindByIdAsync(int id) =>
        _db.MembershipRequests
            .Include(r => r.Member).ThenInclude(m => m.User)
            .Include(r => r.Package)
            .FirstOrDefaultAsync(r => r.Id == id);

    /// <summary>True if the member already has an in-flight request (PENDING or APPROVED).</summary>
    public Task<bool> HasOpenRequestAsync(int memberId) =>
        _db.MembershipRequests.AnyAsync(r =>
            r.MemberId == memberId && (r.Status == "PENDING" || r.Status == "APPROVED"));

    /// <summary>
    /// The request that determines which classes the member may register for:
    /// the most recent ACTIVE membership, or — if none is active yet — the most
    /// recent APPROVED one (so an approved member can preview/register, which
    /// activates it).
    /// </summary>
    public async Task<MembershipRequest?> FindEffectiveAsync(int memberId)
    {
        var active = await _db.MembershipRequests.Include(r => r.Package)
            .Where(r => r.MemberId == memberId && r.Status == "ACTIVE")
            .OrderByDescending(r => r.ActivatedAt)
            .FirstOrDefaultAsync();
        if (active is not null) return active;

        return await _db.MembershipRequests.Include(r => r.Package)
            .Where(r => r.MemberId == memberId && r.Status == "APPROVED")
            .OrderByDescending(r => r.ApprovedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveAsync(MembershipRequest r)
    {
        _db.MembershipRequests.Add(r);
        await _db.SaveChangesAsync();
        return r.Id;
    }

    public async Task UpdateAsync(MembershipRequest r)
    {
        _db.MembershipRequests.Update(r);
        await _db.SaveChangesAsync();
    }
}
