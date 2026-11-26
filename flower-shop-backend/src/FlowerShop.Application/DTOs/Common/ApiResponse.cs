namespace FlowerShop.Application.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Fail(string code, string message, IEnumerable<ApiErrorDetail>? details = null)
        => new() { Success = false, Error = new ApiError(code, message, details) };
}

public record ApiError(string Code, string Message, IEnumerable<ApiErrorDetail>? Details = null);
public record ApiErrorDetail(string Field, string Message);

public class PagedResponse<T>
{
    public bool Success { get; init; }
    public IEnumerable<T> Data { get; init; } = [];
    public PaginationMeta Pagination { get; init; } = new(0, 0, 0, 0);

    public static PagedResponse<T> Ok(IEnumerable<T> data, int page, int pageSize, int total)
        => new()
        {
            Success = true,
            Data = data,
            Pagination = new PaginationMeta(page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize))
        };
}

public record PaginationMeta(int Page, int PageSize, int Total, int TotalPages);
