namespace Contoso.CustomerApi.Services;

/// <summary>
/// Bounds for pagination requests. Centralised so the endpoint, the service and the
/// tests all agree on what a valid page looks like.
/// </summary>
public static class PagingDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    /// <summary>
    /// Clamps caller-supplied paging values into the supported range.
    /// A page below 1 becomes 1; a page size outside the range is clamped, not rejected,
    /// because a client asking for too much should still get a usable answer.
    /// </summary>
    public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
    {
        var p = page.GetValueOrDefault(DefaultPage);
        var s = pageSize.GetValueOrDefault(DefaultPageSize);

        if (p < 1) p = DefaultPage;
        if (s < 1) s = DefaultPageSize;
        if (s > MaxPageSize) s = MaxPageSize;

        return (p, s);
    }
}
