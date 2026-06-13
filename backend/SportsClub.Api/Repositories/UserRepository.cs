using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>
/// DAO PATTERN — all DB operations for the users table. EF Core parameterises
/// every LINQ query, so there is no SQL-injection surface.
/// </summary>
public class UserRepository
{
    private readonly AppDbContext _db;
    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> FindByUsernameAsync(string username) =>
        _db.Users.FirstOrDefaultAsync(u => u.Username == username);

    public Task<User?> FindByEmailAsync(string email) =>
        _db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> FindByIdAsync(int id) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<int> InsertAsync(string username, string passwordHash,
        string email, string? phone, string role)
    {
        var user = new User
        {
            Username = username,
            PasswordHash = passwordHash,
            Email = email,
            Phone = phone,
            Role = role,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user.Id;
    }

    public async Task UpdatePasswordAsync(int userId, string newHash)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;
        user.PasswordHash = newHash;
        await _db.SaveChangesAsync();
    }

    public async Task UpdatePhoneAsync(int userId, string? phone)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;
        user.Phone = phone;
        await _db.SaveChangesAsync();
    }

    // BRUTE-FORCE PROTECTION — log every login attempt.
    // AttemptTime is stamped here in UTC (not left to the DB default) so the
    // lockout window is computed against a single clock regardless of the SQL
    // Server timezone.
    public async Task LogLoginAttemptAsync(string username, string? ip, bool success)
    {
        _db.LoginAttempts.Add(new LoginAttempt
        {
            Username = username,
            IpAddress = ip,
            IsSuccess = success,
            AttemptTime = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Count failed attempts within the last 15 minutes, but only those AFTER
    /// the most recent successful login — so a successful login resets the
    /// lockout counter and a user is not held out by old typos.
    /// </summary>
    public async Task<int> CountRecentFailedAttemptsAsync(string username)
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-15);
        var lastSuccess = await _db.LoginAttempts
            .Where(a => a.Username == username && a.IsSuccess && a.AttemptTime > cutoff)
            .MaxAsync(a => (DateTime?)a.AttemptTime);
        var since = lastSuccess ?? cutoff;
        return await _db.LoginAttempts.CountAsync(a =>
            a.Username == username && !a.IsSuccess && a.AttemptTime > since);
    }
}
