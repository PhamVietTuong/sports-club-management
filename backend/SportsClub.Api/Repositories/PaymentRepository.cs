using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the payments table.</summary>
public class PaymentRepository
{
    private readonly AppDbContext _db;
    public PaymentRepository(AppDbContext db) => _db = db;

    public Task<List<Payment>> FindAllAsync() =>
        _db.Payments.Include(p => p.Member)
            .OrderByDescending(p => p.PaidAt).ToListAsync();

    /// <summary>One page of payments, filtered by status and a member-name /
    /// description / method search.</summary>
    public Task<PagedResult<Payment>> FindPagedAsync(int page, int pageSize, string? search, string? status)
    {
        var q = _db.Payments.Include(p => p.Member).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(p => p.Member.FullName.Contains(s)
                             || p.Method.Contains(s)
                             || (p.Description != null && p.Description.Contains(s)));
        }
        return q.OrderByDescending(p => p.PaidAt).ToPagedResultAsync(page, pageSize);
    }

    public Task<List<Payment>> FindByMemberIdAsync(int memberId) =>
        _db.Payments.Include(p => p.Member)
            .Where(p => p.MemberId == memberId)
            .OrderByDescending(p => p.PaidAt).ToListAsync();

    public Task<PagedResult<Payment>> FindPagedByMemberAsync(int memberId, int page, int pageSize, string? search)
    {
        var q = _db.Payments.Include(p => p.Member).Where(p => p.MemberId == memberId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(p => p.Method.Contains(s) || (p.Description != null && p.Description.Contains(s)));
        }
        return q.OrderByDescending(p => p.PaidAt).ToPagedResultAsync(page, pageSize);
    }

    public async Task<int> SaveAsync(Payment payment)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        return payment.Id;
    }

    /// <summary>Total completed revenue across all time.</summary>
    public async Task<decimal> TotalRevenueAsync() =>
        await _db.Payments.Where(p => p.Status == "COMPLETED")
            .SumAsync(p => (decimal?)p.Amount) ?? 0m;

    /// <summary>Completed revenue grouped by year-month, newest first.</summary>
    public async Task<List<MonthlyRevenue>> MonthlyRevenueAsync()
    {
        // Project to an anonymous type server-side (reliably translatable), then
        // build the record in memory.
        var rows = await _db.Payments
            .Where(p => p.Status == "COMPLETED")
            .GroupBy(p => new { p.PaidAt.Year, p.PaidAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(p => p.Amount), Count = g.Count() })
            .ToListAsync();
        return rows
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .Select(r => new MonthlyRevenue(r.Year, r.Month, r.Total, r.Count))
            .ToList();
    }
}

/// <summary>Aggregated revenue for one calendar month.</summary>
public record MonthlyRevenue(int Year, int Month, decimal Total, int Count);
