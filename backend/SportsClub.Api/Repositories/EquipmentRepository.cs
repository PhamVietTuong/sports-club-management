using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Data;
using SportsClub.Api.Models.Entities;

namespace SportsClub.Api.Repositories;

/// <summary>DAO PATTERN — all DB operations for the equipment table.</summary>
public class EquipmentRepository
{
    private readonly AppDbContext _db;
    public EquipmentRepository(AppDbContext db) => _db = db;

    public Task<List<Equipment>> FindAllAsync() =>
        _db.Equipment.OrderBy(e => e.Id).ToListAsync();

    public Task<List<Equipment>> FindByStatusAsync(string status) =>
        _db.Equipment.Where(e => e.Status == status).OrderBy(e => e.Id).ToListAsync();

    public Task<Equipment?> FindByIdAsync(int id) =>
        _db.Equipment.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<int> SaveAsync(Equipment item)
    {
        _db.Equipment.Add(item);
        await _db.SaveChangesAsync();
        return item.Id;
    }

    public async Task UpdateAsync(Equipment item)
    {
        _db.Equipment.Update(item);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.Equipment.FirstOrDefaultAsync(e => e.Id == id);
        if (item is null) return;
        _db.Equipment.Remove(item);
        await _db.SaveChangesAsync();
    }
}
