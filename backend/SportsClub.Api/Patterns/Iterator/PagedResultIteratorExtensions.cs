using SportsClub.Api.Models.Dtos;

namespace SportsClub.Api.Patterns.Iterator;

/// <summary>
/// ITERATOR PATTERN bridge for paginated results. List endpoints still traverse
/// their page of rows via <see cref="ClubCollection{T}"/> / <see cref="ClubIterator{T}"/>
/// (as the original Java servlets did) while mapping to DTOs — the only change
/// from the non-paged endpoints is that just one page is iterated.
/// </summary>
public static class PagedResultIteratorExtensions
{
    public static PagedResult<TOut> MapIterating<TIn, TOut>(
        this PagedResult<TIn> page, Func<TIn, TOut> map) =>
        new(ClubCollection<TIn>.Of(page.Items).Select(map).ToList(),
            page.Total, page.Page, page.PageSize);
}
