using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Dtos;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the training_packages table.</summary>
public class PackageRepository
{
    private readonly AppDbContext _db;
    public PackageRepository(AppDbContext db) => _db = db;

    public Task<List<TrainingPackage>> FindAllAsync() =>
        _db.TrainingPackages.OrderBy(p => p.Id).ToListAsync();

    /// <summary>One page of packages, filtered by a name/description search.</summary>
    public Task<PagedResult<TrainingPackage>> FindPagedAsync(int page, int pageSize, string? search)
    {
        var q = _db.TrainingPackages.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(p => p.Name.Contains(s) || (p.Description != null && p.Description.Contains(s)));
        }
        return q.OrderBy(p => p.Id).ToPagedResultAsync(page, pageSize);
    }

    public Task<List<TrainingPackage>> FindActiveAsync() =>
        _db.TrainingPackages.Where(p => p.IsActive).OrderBy(p => p.Price).ToListAsync();

    public Task<TrainingPackage?> FindByIdAsync(int id) =>
        _db.TrainingPackages.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<int> SaveAsync(TrainingPackage pkg)
    {
        _db.TrainingPackages.Add(pkg);
        await _db.SaveChangesAsync();
        return pkg.Id;
    }

    public async Task UpdateAsync(TrainingPackage pkg)
    {
        _db.TrainingPackages.Update(pkg);
        await _db.SaveChangesAsync();
    }
}
