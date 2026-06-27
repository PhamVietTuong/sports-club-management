using Microsoft.EntityFrameworkCore;
using SportsClub.Api.Models.Dtos;

namespace SportsClub.Api.Repositories;

/// <summary>Server-side pagination helper for the DAO layer.</summary>
public static class PagingExtensions
{
    /// <summary>
    /// Run a COUNT for the total, then fetch one page (Skip/Take). page is
    /// 1-based; pageSize is clamped to [1, 100]. EF Core translates both to SQL,
    /// so only one page of rows is materialized.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize;

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<T>(items, total, page, pageSize);
    }
}
