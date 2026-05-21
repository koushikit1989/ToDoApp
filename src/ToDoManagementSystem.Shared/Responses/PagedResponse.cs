namespace ToDoManagementSystem.Shared.Responses;

/// <summary>Paginated response wrapper.</summary>
public class PagedResponse<T>
{
    /// <summary>The page of data items.</summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>Total record count across all pages.</summary>
    public int TotalCount { get; set; }

    /// <summary>Current page number (1-based).</summary>
    public int PageNumber { get; set; }

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>Whether there is a previous page.</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Whether there is a next page.</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>Factory method to create a paged response.</summary>
    public static PagedResponse<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize) =>
        new() { Items = items, TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize };
}
