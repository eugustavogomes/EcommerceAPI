namespace ECommerceAPI.API.Extensions;

public static class PaginationExtensions
{
    public static void AddPaginationHeaders(
        this HttpResponse response, int totalCount, int pageNumber, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        response.Headers.Append("X-Total-Count", totalCount.ToString());
        response.Headers.Append("X-Page", pageNumber.ToString());
        response.Headers.Append("X-Page-Size", pageSize.ToString());
        response.Headers.Append("X-Total-Pages", totalPages.ToString());
        response.Headers.Append("Access-Control-Expose-Headers",
            "X-Total-Count,X-Page,X-Page-Size,X-Total-Pages");
    }
}
