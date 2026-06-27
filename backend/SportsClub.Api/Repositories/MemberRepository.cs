using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the members table.</summary>
public class MemberRepository
{
    private readonly AppDbContext _db;
    public MemberRepository(AppDbContext db) => _db = db;

    public Task<List<Member>> FindAllAsync() =>
        _db.Members.Include(m => m.User)
            .OrderByDescending(m => m.JoinDate).ToListAsync();

    /// <summary>One page of members, filtered by status and a free-text search
    /// over name / username / email.</summary>
    public Task<PagedResult<Member>> FindPagedAsync(int page, int pageSize, string? search, string? status)
    {
        var q = _db.Members.Include(m => m.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(m => m.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(m => m.FullName.Contains(s)
                             || m.User.Username.Contains(s)
                             || m.User.Email.Contains(s));
        }
        return q.OrderByDescending(m => m.JoinDate).ToPagedResultAsync(page, pageSize);
    }

    public Task<List<Member>> FindByStatusAsync(string status) =>
        _db.Members.Include(m => m.User)
            .Where(m => m.Status == status)
            .OrderByDescending(m => m.JoinDate).ToListAsync();

    public Task<Member?> FindByIdAsync(int id) =>
        _db.Members.Include(m => m.User).FirstOrDefaultAsync(m => m.Id == id);

    public Task<Member?> FindByUserIdAsync(int userId) =>
        _db.Members.Include(m => m.User).FirstOrDefaultAsync(m => m.UserId == userId);

    public async Task<int> InsertAsync(int userId, string fullName, string? gender,
        DateOnly? dateOfBirth, string? address, int packageId, DateOnly? expiryDate)
    {
        var member = new Member
        {
            UserId = userId,
            FullName = fullName,
            Gender = gender,
            DateOfBirth = dateOfBirth,
            Address = address,
            PackageId = packageId,
            JoinDate = DateOnly.FromDateTime(DateTime.Today),
            ExpiryDate = expiryDate,
            Status = "ACTIVE",
        };
        _db.Members.Add(member);
        await _db.SaveChangesAsync();
        return member.Id;
    }

    public async Task UpdateAsync(Member m)
    {
        // Mark only the member entity Modified. Using _db.Members.Update(m)
        // would traverse the graph and also mark an Included User Modified,
        // rewriting the users row unnecessarily.
        _db.Entry(m).State = EntityState.Modified;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == id);
        if (member is null) return;
        member.Status = status;
        await _db.SaveChangesAsync();
    }

    public Task<int> CountAllAsync() => _db.Members.CountAsync();
}
