using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public abstract class GenericRepository<TEntity, TDetailDto, TNewDto,
			TUpdateDto>
		: LookupRepository<TEntity, TDetailDto>
		where TEntity : class, IEntity
	{
		/// <inheritdoc />
		protected GenericRepository(DataContext context, IMapper mapper) : base(context,
			mapper)
		{
		}

		public Task<long> CreateAsync(TNewDto dto, CancellationToken cancellationToken)
		{
			return CreateFromDtoAsync(dto, cancellationToken);
		}

		public Task<bool> UpdateAsync(long id, TUpdateDto dto, CancellationToken cancellationToken)
		{
			return UpdateFromDtoAsync(id, dto, cancellationToken);
		}
	}
}