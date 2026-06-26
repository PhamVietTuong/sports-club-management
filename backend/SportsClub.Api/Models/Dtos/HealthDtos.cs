using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record HealthMetricDto(
    int Id,
    DateOnly RecordedDate,
    decimal? WeightKg,
    decimal? HeightCm,
    decimal? BodyFatPct,
    string? Notes)
{
    public static HealthMetricDto From(HealthMetric h) => new(
        h.Id, h.RecordedDate, h.WeightKg, h.HeightCm, h.BodyFatPct, h.Notes);
}

public record SaveHealthMetricRequest(
    [param: Required] DateOnly RecordedDate,
    [param: Range(0, 500)] decimal? WeightKg,
    [param: Range(0, 300)] decimal? HeightCm,
    [param: Range(0, 100)] decimal? BodyFatPct,
    string? Notes);
