namespace HexagonalArchitecture.Domain.Exceptions;
public class BaseException : Exception
{
    public int StatusCode { get; private set; }
    public string MessageKey { get; private set; }
    public object[] MessageArgs { get; private set; }

    public BaseException(string messageKey, int statusCode = 400, params object[] messageArgs)
        : base(messageKey)
    {
        MessageKey = messageKey;
        StatusCode = statusCode;
        MessageArgs = messageArgs;
    }
}