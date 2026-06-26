using System.ComponentModel.DataAnnotations;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Models.Dtos;

public record EquipmentDto(
    int Id,
    string Name,
    string? Category,
    int Quantity,
    string Status,
    DateOnly? PurchaseDate,
    string? Notes)
{
    public static EquipmentDto From(Equipment e) => new(
        e.Id, e.Name, e.Category, e.Quantity, e.Status, e.PurchaseDate, e.Notes);

    /// <summary>Statuses allowed by the equipment table CHECK constraint.</summary>
    public static readonly string[] AllowedStatuses =
        { "AVAILABLE", "IN_USE", "MAINTENANCE", "RETIRED" };
}

public record SaveEquipmentRequest(
    [param: Required] string Name,
    string? Category,
    int Quantity,
    string? Status,
    DateOnly? PurchaseDate,
    string? Notes);
