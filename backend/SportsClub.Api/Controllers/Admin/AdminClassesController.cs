using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/classes")]
[Authorize(Roles = UserRole.Admin)]
public class AdminClassesController : ControllerBase
{
    private readonly ClassRepository _classes;

    public AdminClassesController(ClassRepository classes) => _classes = classes;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClassDto>>> List()
    {
        // ITERATOR PATTERN — traverse classes via the club iterator
        var classes = ClubCollection<TrainingClass>.Of(await _classes.FindAllAsync());
        return Ok(classes.Select(ClassDto.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateClassRequest req)
    {
        await _classes.InsertAsync(new TrainingClass
        {
            Name = req.Name,
            CoachId = req.CoachId == 0 ? null : req.CoachId,
            Capacity = req.Capacity,
            Level = req.Level,
            Description = req.Description,
            IsActive = true,
        });
        return Ok(new MessageResponse("Đã thêm lớp học."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateClassRequest req)
    {
        var tc = await _classes.FindByIdAsync(id);
        if (tc is null) return NotFound(new MessageResponse("Không tìm thấy lớp học."));

        tc.Name = req.Name;
        tc.CoachId = req.CoachId == 0 ? null : req.CoachId;
        tc.Capacity = req.Capacity;
        tc.Level = req.Level;
        tc.Description = req.Description;
        tc.IsActive = req.IsActive;
        await _classes.UpdateAsync(tc);

        return Ok(new MessageResponse("Đã cập nhật lớp học."));
    }

    /// <summary>PROTOTYPE PATTERN — duplicate a class template.</summary>
    [HttpPost("{id:int}/clone")]
    public async Task<IActionResult> Clone(int id)
    {
        var template = await _classes.FindByIdAsync(id);
        if (template is null) return NotFound(new MessageResponse("Không tìm thấy lớp học."));

        var copy = template.Clone();   // PROTOTYPE in action
        copy.Id = 0;
        copy.Name = "Copy of " + template.Name;
        copy.CurrentEnrolled = 0;
        copy.Coach = null;
        await _classes.InsertAsync(copy);

        return Ok(new MessageResponse("Đã nhân bản lớp học."));
    }
}
