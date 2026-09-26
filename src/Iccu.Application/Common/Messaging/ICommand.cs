namespace Iccu.Application.Common.Messaging;

using MediatR;
using Iccu.Domain.Common;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
