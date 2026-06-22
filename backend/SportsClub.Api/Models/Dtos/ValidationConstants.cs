namespace SportsClub.Api.Models.Dtos;

/// <summary>Shared validation rules so DTOs format phone/email consistently.</summary>
public static class ValidationConstants
{
    /// <summary>
    /// Vietnamese phone number: starts with <c>0</c> or <c>+84</c> followed by 9 digits
    /// (e.g. <c>0901234567</c> or <c>+84901234567</c>). Applied only to non-empty values
    /// since the phone field is optional.
    /// </summary>
    public const string PhonePattern = @"^(0|\+84)\d{9}$";

    public const string PhoneError = "Số điện thoại không hợp lệ (định dạng: 0xxxxxxxxx hoặc +84xxxxxxxxx).";
}
