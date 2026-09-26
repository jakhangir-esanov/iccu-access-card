namespace Iccu.Application.Common.Messaging;

using MediatR;
using Iccu.Domain.Common;
using Iccu.Application.Common.Paging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface IPagedListQuery<TResponse> : IRequest<PagedList<TResponse>>;
