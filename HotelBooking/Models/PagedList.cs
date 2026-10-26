using System.Collections;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Models;

public interface IPagedList
{
    int Page { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
}

public sealed class PagedList<T> : IReadOnlyList<T>, IPagedList
{
    private readonly IReadOnlyList<T> items;
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => TotalCount == 0 ? 1 : (TotalCount - 1) / PageSize + 1;
    public int Count => items.Count;
    public T this[int index] => items[index];

    private PagedList(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        this.items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public static PagedList<T> Empty(int pageSize) => new([], 1, pageSize, 0);

    // Callers supply a deterministic ordering, including a unique key for equal values.
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize);
        var total = await query.CountAsync(cancellationToken);
        var pages = total == 0 ? 1 : (total - 1) / pageSize + 1;
        page = Math.Clamp(page, 1, pages);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new(items, page, pageSize, total);
    }

    public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
