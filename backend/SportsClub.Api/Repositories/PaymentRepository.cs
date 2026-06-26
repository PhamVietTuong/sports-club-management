using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
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

    public Task<List<Payment>> FindByMemberIdAsync(int memberId) =>
        _db.Payments.Include(p => p.Member)
            .Where(p => p.MemberId == memberId)
            .OrderByDescending(p => p.PaidAt).ToListAsync();

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
