namespace HexagonalArchitecture.Domain.Exceptions;
public class BusinessException : Exception
{
    public int StatusCode { get; }
    public string MessageKey { get; }
    public BusinessException(string messageKey, int statusCode = 400) : base(messageKey)
    {
        MessageKey = messageKey;
        StatusCode = statusCode;
    }
}