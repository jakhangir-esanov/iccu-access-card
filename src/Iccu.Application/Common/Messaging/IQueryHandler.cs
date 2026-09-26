namespace Iccu.Application.Common.Messaging;

using MediatR;
using Iccu.Domain.Common;
using Iccu.Application.Common.Paging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;

public interface IPagedListQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, PagedList<TResponse>>
    where TQuery : IPagedListQuery<TResponse>;
