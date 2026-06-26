using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Repositories;

namespace SportsClub.Api.Models.Dtos;

public record PaymentDto(
    int Id,
    int MemberId,
    string MemberName,
    int? PackageId,
    decimal Amount,
    string Method,
    string Status,
    string? Description,
    DateTime PaidAt)
{
    public static PaymentDto From(Payment p) => new(
        p.Id, p.MemberId, p.Member?.FullName ?? "", p.PackageId, p.Amount,
        p.Method, p.Status, p.Description, p.PaidAt);
}

/// <summary>A member buying a membership package. Method must be CASH/CARD/TRANSFER.</summary>
public record BuyMembershipRequest(
    [param: Required] int PackageId,
    string? Method)
{
    public static readonly string[] AllowedMethods = { "CASH", "CARD", "TRANSFER" };
}

public record MonthlyRevenueDto(int Year, int Month, decimal Total, int Count)
{
    public static MonthlyRevenueDto From(MonthlyRevenue r) => new(r.Year, r.Month, r.Total, r.Count);
}

public record RevenueDto(decimal Total, int PaymentCount, IEnumerable<MonthlyRevenueDto> Monthly);
