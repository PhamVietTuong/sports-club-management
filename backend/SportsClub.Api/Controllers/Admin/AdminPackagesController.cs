using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;
using SportsClub.Api.Patterns.Iterator;
using SportsClub.Api.Repositories;

namespace SportsClub.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/packages")]
[Authorize(Roles = UserRole.Admin)]
public class AdminPackagesController : ControllerBase
{
    private readonly PackageRepository _packages;
    private readonly PackageClassRepository _packageClasses;

    public AdminPackagesController(PackageRepository packages, PackageClassRepository packageClasses)
    {
        _packages = packages;
        _packageClasses = packageClasses;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PackageDto>>> List(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _packages.FindPagedAsync(page, pageSize, search);
        // ITERATOR PATTERN — traverse the page via the club iterator while mapping.
        return Ok(result.MapIterating(PackageDto.From));
    }

    [HttpPost]
    public async Task<IActionResult> Create(SavePackageRequest req)
    {
        await _packages.SaveAsync(new TrainingPackage
        {
            Name = req.Name,
            DurationMonths = req.DurationMonths,
            Price = req.Price,
            MaxClasses = req.MaxClasses,
            Description = req.Description,
            IsActive = true,
        });
        return Ok(new MessageResponse("Đã thêm gói tập."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdatePackageRequest req)
    {
        var pkg = await _packages.FindByIdAsync(id);
        if (pkg is null) return NotFound(new MessageResponse("Không tìm thấy gói tập."));

        pkg.Name = req.Name;
        pkg.DurationMonths = req.DurationMonths;
        pkg.Price = req.Price;
        pkg.MaxClasses = req.MaxClasses;
        pkg.Description = req.Description;
        pkg.IsActive = req.IsActive;
        await _packages.UpdateAsync(pkg);

        return Ok(new MessageResponse("Đã cập nhật gói tập."));
    }

    /// <summary>PROTOTYPE PATTERN — clone a package template at a 20% premium.</summary>
    [HttpPost("{id:int}/clone")]
    public async Task<IActionResult> Clone(int id)
    {
        var template = await _packages.FindByIdAsync(id);
        if (template is null) return NotFound(new MessageResponse("Không tìm thấy gói tập."));

        var copy = template.Clone();   // PROTOTYPE in action
        copy.Id = 0;
        copy.Name = "Copy of " + template.Name;
        copy.Price = template.Price * 1.2m;   // 20% premium
        await _packages.SaveAsync(copy);

        return Ok(new MessageResponse("Đã nhân bản gói tập."));
    }

    /// <summary>The ids of the classes this package grants access to.</summary>
    [HttpGet("{id:int}/classes")]
    public async Task<IActionResult> GetClasses(int id)
    {
        if (await _packages.FindByIdAsync(id) is null)
            return NotFound(new MessageResponse("Không tìm thấy gói tập."));
        return Ok(new { classIds = await _packageClasses.FindClassIdsAsync(id) });
    }

    /// <summary>Set exactly which classes this package grants access to.</summary>
    [HttpPut("{id:int}/classes")]
    public async Task<IActionResult> SetClasses(int id, SetPackageClassesRequest req)
    {
        if (await _packages.FindByIdAsync(id) is null)
            return NotFound(new MessageResponse("Không tìm thấy gói tập."));
        await _packageClasses.SetAsync(id, req.ClassIds ?? Array.Empty<int>());
        return Ok(new MessageResponse("Đã cập nhật danh sách lớp của gói tập."));
    }
}
