using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexagonalArchitecture.Domain.Exceptions;
public class ValidationException : BaseException
{
    public ValidationException(string messageKey, params object[] args) : base(messageKey, 400, args) { }
}