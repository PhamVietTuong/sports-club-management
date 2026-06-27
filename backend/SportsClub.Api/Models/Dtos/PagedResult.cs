namespace SportsClub.Api.Models.Dtos;

/// <summary>
/// A single page of a larger result set. Serialized to the SPA as
/// { items, total, page, pageSize } so tables can render server-side
/// pagination + filtering.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    /// <summary>Project the page's items to a DTO, keeping the paging metadata.</summary>
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> selector) =>
        new(Items.Select(selector).ToList(), Total, Page, PageSize);
}
