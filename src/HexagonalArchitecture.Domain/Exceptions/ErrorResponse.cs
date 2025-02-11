using HexagonalArchitecture.Domain.Middlewares;

namespace HexagonalArchitecture.Domain.Exceptions;
public class ErrorResponse
{
    public string TraceId { get; set; }
    public DateTime Timestamp { get; set; }
    public int Status { get; set; }
    public ErrorDetail Error { get; set; }
    public DebugInfo Debug { get; set; }
}