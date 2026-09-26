namespace Iccu.Application.Common.Messaging;

using MediatR;
using Iccu.Domain.Common;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface IBaseCommand;
