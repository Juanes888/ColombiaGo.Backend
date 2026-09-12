using System.Diagnostics.CodeAnalysis;

namespace ColombiaGo.Application.Common;

/// <summary>A page of results plus enough metadata for the client to build pagination controls.</summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    [SuppressMessage("Design", "CA1000", Justification =
        "Empty() is a convenience factory for the empty-page case; it's meant to be called as " +
        "PagedResult<T>.Empty(...), same shape as Result<T>'s factories.")]
    public static PagedResult<T> Empty(int page, int pageSize) => new(Array.Empty<T>(), page, pageSize, 0);
}
