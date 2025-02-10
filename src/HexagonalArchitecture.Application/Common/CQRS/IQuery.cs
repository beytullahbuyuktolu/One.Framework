using HexagonalArchitecture.Application.Common.Models;
using MediatR;

namespace HexagonalArchitecture.Application.Common.CQRS;

public interface IQuery<TResponse> : IRequest<BaseResponse<TResponse>>
{
}
