using System.ComponentModel.DataAnnotations;

namespace SportsClub.Api.Models.Dtos;

public record LoginRequest(
    [param: Required] string Username,
    [param: Required] string Password);

public record RegisterRequest(
    [param: Required] string Username,
    [param: Required, EmailAddress] string Email,
    [param: Required] string Password,
    [param: Required] string ConfirmPassword,
    [param: Required] string FullName,
    [param: RegularExpression(ValidationConstants.PhonePattern, ErrorMessage = ValidationConstants.PhoneError)]
    string? Phone,
    string? Gender);

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    int UserId,
    string Username,
    string Role,
    string FullName);
