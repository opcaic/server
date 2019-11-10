using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Infrastructure.Queries
{
	/// <summary>
	///     Base class for filter oriented query handlers.
	/// </summary>
	/// <typeparam name="TRequest">Type of the request.</typeparam>
	/// <typeparam name="TEntity">Type of the entity queried.</typeparam>
	/// <typeparam name="TResult">Type of the result DTO.</typeparam>
	public abstract class
		FilterQueryHandler<TRequest, TEntity, TResult>
		: IRequestHandler<TRequest, PagedResult<TResult>>
		where TRequest : FilterDtoBase, IRequest<PagedResult<TResult>>
	{
		protected readonly IMapper Mapper;
		protected readonly IRepository<TEntity> Repository;

		protected FilterQueryHandler(IMapper mapper, IRepository<TEntity> repository)
		{
			Mapper = mapper;
			Repository = repository;
		}

		/// <inheritdoc />
		public virtual Task<PagedResult<TResult>> Handle(TRequest request,
			CancellationToken cancellationToken)
		{
			var spec = ProjectingSpecification<TEntity>.Create<TResult>(Mapper);
			spec.WithPaging(request.Offset, request.Count);

			// admins must be able to see everything
			if (request.RequestingUserRole != UserRole.Admin)
			{
				ApplyUserFilter(spec, request);
			}

			SetupSpecification(request, spec);

			return Repository.ListPagedAsync(spec, cancellationToken);
		}

		/// <summary>
		///     Applies data viewing restriction for the current user
		/// </summary>
		/// <param name="spec">The specification for the db query.</param>
		/// <param name="request"></param>
		protected abstract void ApplyUserFilter(ProjectingSpecification<TEntity, TResult> spec,
			TRequest request);

		/// <summary>
		///     Sets up the specification for db query based on the request data.
		/// </summary>
		/// <param name="request">The request for data.</param>
		/// <param name="spec">Specification to be fit with criteria.</param>
		protected abstract void SetupSpecification(TRequest request,
			ProjectingSpecification<TEntity, TResult> spec);
	}
}