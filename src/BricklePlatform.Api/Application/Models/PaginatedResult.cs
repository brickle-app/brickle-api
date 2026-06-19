namespace BricklePlatform.Api.Application.Models;

public class PaginatedResult<T>
{
    public IEnumerable<T> Data { get; set; }
    public PaginationInfo Pagination { get; set; }

    public PaginatedResult(IEnumerable<T> data, int currentPage, int totalPages, int totalItems)
    {
        Data = data;
        Pagination = new PaginationInfo
        {
            CurrentPage = currentPage,
            TotalPages = totalPages,
            TotalItems = totalItems
        };
    }
}

public class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
} 