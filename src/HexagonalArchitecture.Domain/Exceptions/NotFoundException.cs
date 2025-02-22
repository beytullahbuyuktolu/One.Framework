namespace HexagonalArchitecture.Domain.Exceptions;
public class NotFoundException : BusinessException
{
    public NotFoundException(string messageKey) : base(messageKey, 404)    {    }
}