using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Common;

/// <summary>Filter/paging parameters for listing <c>lugares</c> — backs the Explore screen's
/// search box, category chips, and the Home screen's "ciudad" deep link.</summary>
public sealed class LugarQueryParams
{
    public string? Q { get; init; }
    public string? CategoriaSlug { get; init; }
    public string? Ciudad { get; init; }
    public string? Region { get; init; }
    public RangoPrecio? RangoPrecio { get; init; }

    private const int MaxPageSize = 100;
    private int _page = 1;
    private int _pageSize = 20;

    public int Page
    {
        get => _page;
        init => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value switch { < 1 => 1, > MaxPageSize => MaxPageSize, _ => value };
    }
}
