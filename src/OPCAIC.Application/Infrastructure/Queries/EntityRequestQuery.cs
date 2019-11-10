using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Infrastructure.Queries
{
	public abstract class EntityRequestQuery<TEntity> : PublicRequest
		where TEntity : class, IEntity
	{
		protected EntityRequestQuery(long id)
		{
			Id = id;
		}

		public long Id { get; }

		public abstract class EntityRequestHandler<TRequest, TResponse>
			: IRequestHandler<TRequest, TResponse>
			where TRequest : EntityRequestQuery<TEntity>, IRequest<TResponse>
			where TResponse : class
		{
			protected readonly IMapper mapper;
			protected readonly IRepository<TEntity> repository;

			/// <inheritdoc />
			protected EntityRequestHandler(IMapper mapper, IRepository<TEntity> repository)
			{
				this.mapper = mapper;
				this.repository = repository;
			}

			/// <inheritdoc />
			public virtual Task<TResponse> Handle(TRequest request,
				CancellationToken cancellationToken)
			{
				return repository.GetAsync<TEntity, TResponse>(request.Id, mapper,
					cancellationToken);
			}
		}
	}
}