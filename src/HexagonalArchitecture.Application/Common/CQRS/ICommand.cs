using HexagonalArchitecture.Application.Common.Models;
using MediatR;

namespace HexagonalArchitecture.Application.Common.CQRS;

public interface ICommand<TResponse> : IRequest<BaseResponse<TResponse>>
{
}
