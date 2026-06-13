using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record MemberDto(
    int Id,
    int UserId,
    string Username,
    string Email,
    string? Phone,
    string FullName,
    string? Gender,
    DateOnly? DateOfBirth,
    string? Address,
    int PackageId,
    DateOnly JoinDate,
    DateOnly? ExpiryDate,
    string Status)
{
    public static MemberDto From(Member m) => new(
        m.Id, m.UserId, m.User?.Username ?? "", m.User?.Email ?? "", m.User?.Phone,
        m.FullName, m.Gender, m.DateOfBirth, m.Address, m.PackageId,
        m.JoinDate, m.ExpiryDate, m.Status);
}

public record CreateMemberRequest(
    [param: Required] string Username,
    [param: Required, EmailAddress] string Email,
    [param: Required] string Password,
    [param: Required] string FullName,
    string? Phone,
    string? Gender,
    string? Address,
    DateOnly? DateOfBirth,
    DateOnly? ExpiryDate,
    int? PackageId);

public record UpdateMemberStatusRequest([param: Required] string Status);
