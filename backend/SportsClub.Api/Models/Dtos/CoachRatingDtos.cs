using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

/// <summary>A coach as seen by a member: rating stats + this member's own rating.</summary>
public record RateableCoachDto(
    int Id,
    string FullName,
    string? Specialization,
    int Experience,
    double AverageRating,
    int RatingCount,
    int? MyRating,
    string? MyComment,
    bool CanRate);

/// <summary>One rating row a coach sees about themselves.</summary>
public record CoachRatingDto(
    int Id,
    int MemberId,
    string MemberName,
    int Rating,
    string? Comment,
    DateTime CreatedAt)
{
    public static CoachRatingDto From(CoachRating r) => new(
        r.Id, r.MemberId, r.Member?.FullName ?? "", r.Rating, r.Comment, r.CreatedAt);
}

public record CoachRatingSummaryDto(double Average, int Count, IEnumerable<CoachRatingDto> Ratings);

public record RateCoachRequest(
    [param: Required, Range(1, 5)] int Rating,
    string? Comment);
