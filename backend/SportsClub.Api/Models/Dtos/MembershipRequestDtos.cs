using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record MembershipRequestDto(
    int Id,
    int MemberId,
    string MemberName,
    int PackageId,
    string PackageName,
    decimal Amount,
    string Method,
    string Status,
    DateTime RequestedAt,
    DateTime? ApprovedAt,
    DateOnly? StartDate,
    DateTime? ActivatedAt,
    string? Note,
    bool CanModify)
{
    public static MembershipRequestDto From(MembershipRequest r) => new(
        r.Id, r.MemberId, r.Member?.FullName ?? "", r.PackageId, r.Package?.Name ?? "",
        r.Amount, r.Method, r.Status, r.RequestedAt, r.ApprovedAt, r.StartDate, r.ActivatedAt,
        r.Note, r.IsModifiable());
}

/// <summary>Member changes the package on an in-flight (modifiable) request.</summary>
public record ChangePackageRequest([param: Required] int PackageId);

/// <summary>Admin approve/reject body — an optional note (e.g. rejection reason).</summary>
public record DecisionRequest(string? Note);
