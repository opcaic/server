using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Specifications;

namespace OPCAIC.Application.Infrastructure.Queries
{
	public abstract class FilterQueryHandler<TRequest, TEntity, TResult> : IRequestHandler<TRequest, PagedResult<TResult>> where TRequest : FilterDtoBase, IRequest<PagedResult<TResult>>
	{
		protected readonly IMapper mapper;
		protected readonly IRepository<TEntity> repository;

		protected FilterQueryHandler(IMapper mapper, IRepository<TEntity> repository)
		{
			this.mapper = mapper;
			this.repository = repository;
		}

		/// <inheritdoc />
		public virtual Task<PagedResult<TResult>> Handle(TRequest request, CancellationToken cancellationToken)
		{
			var spec = ProjectingSpecification<TEntity>.Create<TResult>(mapper);
			spec.WithPaging(request.Offset, request.Count);

			SetupSpecification(request, spec);

			return repository.ListPagedAsync(spec, cancellationToken);
		}

		protected abstract void SetupSpecification(TRequest request, ProjectingSpecification<TEntity, TResult> spec);
	}
}