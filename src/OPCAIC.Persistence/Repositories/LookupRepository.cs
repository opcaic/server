using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public abstract class LookupRepository<TEntity, TDetailDto>
		: EntityRepository<TEntity>
		where TEntity : class, IEntity
	{
		/// <inheritdoc />
		protected LookupRepository(DataContext context, IMapper mapper) : base(context,
			mapper)
		{
		}

		public Task<TDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<TDetailDto>(id, cancellationToken);
		}
	}
}