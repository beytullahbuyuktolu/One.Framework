namespace HexagonalArchitecture.Domain.Exceptions;
public class ErrorDetail
{
    public string Code { get; set; }
    public string Message { get; set; }
    public object Details { get; set; }
}