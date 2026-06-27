using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — manages the package_classes link table (which classes
/// each package grants access to).</summary>
public class PackageClassRepository
{
    private readonly AppDbContext _db;
    public PackageClassRepository(AppDbContext db) => _db = db;

    /// <summary>Ids of the classes linked to a package.</summary>
    public Task<List<int>> FindClassIdsAsync(int packageId) =>
        _db.PackageClasses.Where(pc => pc.PackageId == packageId)
            .Select(pc => pc.ClassId).ToListAsync();

    public Task<bool> IsLinkedAsync(int packageId, int classId) =>
        _db.PackageClasses.AnyAsync(pc => pc.PackageId == packageId && pc.ClassId == classId);

    /// <summary>The active classes a package grants access to (with their coach).</summary>
    public async Task<List<TrainingClass>> FindClassesAsync(int packageId)
    {
        var ids = await FindClassIdsAsync(packageId);
        return await _db.TrainingClasses.Include(c => c.Coach)
            .Where(c => c.IsActive && ids.Contains(c.Id))
            .OrderBy(c => c.Id).ToListAsync();
    }

    /// <summary>Replace the package's class links with exactly the given set.</summary>
    public async Task SetAsync(int packageId, IEnumerable<int> classIds)
    {
        var existing = await _db.PackageClasses.Where(pc => pc.PackageId == packageId).ToListAsync();
        _db.PackageClasses.RemoveRange(existing);
        foreach (var classId in classIds.Distinct())
            _db.PackageClasses.Add(new PackageClass { PackageId = packageId, ClassId = classId });
        await _db.SaveChangesAsync();
    }
}
