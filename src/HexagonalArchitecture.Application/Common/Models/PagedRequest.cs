namespace HexagonalArchitecture.Application.Common.Models;

public class PagedRequest
{
    private int _pageSize = 10;
    private int _pageIndex = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value <= 0 ? 10 : value;
    }

    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value <= 0 ? 1 : value;
    }

    public string SortBy { get; set; } = string.Empty;
    public bool SortDesc { get; set; }
}
