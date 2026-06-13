using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record PackageDto(
    int Id,
    string Name,
    int DurationMonths,
    decimal Price,
    int MaxClasses,
    string? Description,
    bool IsActive)
{
    public static PackageDto From(TrainingPackage p) => new(
        p.Id, p.Name, p.DurationMonths, p.Price, p.MaxClasses, p.Description, p.IsActive);
}

public record SavePackageRequest(
    [param: Required] string Name,
    int DurationMonths,
    decimal Price,
    int MaxClasses,
    string? Description);

public record UpdatePackageRequest(
    [param: Required] string Name,
    int DurationMonths,
    decimal Price,
    int MaxClasses,
    string? Description,
    bool IsActive);
