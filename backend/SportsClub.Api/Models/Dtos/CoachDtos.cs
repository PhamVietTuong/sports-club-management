using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record CoachDto(
    int Id,
    int UserId,
    string Username,
    string Email,
    string? Phone,
    string FullName,
    string? Specialization,
    string? Bio,
    int Experience,
    decimal Salary)
{
    public static CoachDto From(Coach c) => new(
        c.Id, c.UserId, c.User?.Username ?? "", c.User?.Email ?? "", c.User?.Phone,
        c.FullName, c.Specialization, c.Bio, c.Experience, c.Salary);
}

public record CreateCoachRequest(
    [param: Required] string Username,
    [param: Required, EmailAddress] string Email,
    [param: Required] string Password,
    [param: Required] string FullName,
    [param: RegularExpression(ValidationConstants.PhonePattern, ErrorMessage = ValidationConstants.PhoneError)]
    string? Phone,
    string? Specialization,
    string? Bio,
    int Experience,
    decimal Salary);

public record UpdateCoachRequest(
    [param: Required] string FullName,
    string? Specialization,
    string? Bio,
    int Experience,
    decimal Salary);
