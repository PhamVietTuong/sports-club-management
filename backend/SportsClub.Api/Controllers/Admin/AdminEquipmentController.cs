using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/equipment")]
[Authorize(Roles = UserRole.Admin)]
public class AdminEquipmentController : ControllerBase
{
    private readonly EquipmentRepository _equipment;

    public AdminEquipmentController(EquipmentRepository equipment) => _equipment = equipment;

    [HttpGet]
    public async Task<ActionResult<PagedResult<EquipmentDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null, [FromQuery] string? status = null)
    {
        var result = await _equipment.FindPagedAsync(page, pageSize, search, status);
        // ITERATOR PATTERN — traverse the page via the club iterator while mapping.
        return Ok(result.MapIterating(EquipmentDto.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create(SaveEquipmentRequest req)
    {
        var status = NormalizeStatus(req.Status);
        if (status is null) return BadRequest(new MessageResponse("Trạng thái không hợp lệ."));

        await _equipment.SaveAsync(new Equipment
        {
            Name = req.Name.Trim(),
            Category = req.Category,
            Quantity = req.Quantity < 0 ? 0 : req.Quantity,
            Status = status,
            PurchaseDate = req.PurchaseDate,
            Notes = req.Notes,
        });
        return Ok(new MessageResponse("Đã thêm thiết bị."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveEquipmentRequest req)
    {
        var status = NormalizeStatus(req.Status);
        if (status is null) return BadRequest(new MessageResponse("Trạng thái không hợp lệ."));

        var item = await _equipment.FindByIdAsync(id);
        if (item is null) return NotFound(new MessageResponse("Không tìm thấy thiết bị."));

        item.Name = req.Name.Trim();
        item.Category = req.Category;
        item.Quantity = req.Quantity < 0 ? 0 : req.Quantity;
        item.Status = status;
        item.PurchaseDate = req.PurchaseDate;
        item.Notes = req.Notes;
        await _equipment.UpdateAsync(item);

        return Ok(new MessageResponse("Đã cập nhật thiết bị."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (await _equipment.FindByIdAsync(id) is null)
            return NotFound(new MessageResponse("Không tìm thấy thiết bị."));
        await _equipment.DeleteAsync(id);
        return Ok(new MessageResponse("Đã xóa thiết bị."));
    }

    // Default to AVAILABLE when unset; reject anything outside the CHECK constraint.
    private static string? NormalizeStatus(string? status)
    {
        if (string.IsNullOrEmpty(status)) return "AVAILABLE";
        return EquipmentDto.AllowedStatuses.Contains(status) ? status : null;
    }
}
