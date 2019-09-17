using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	/// <summary>
	///     Base class for repository of <see cref="TEntity" /> instances.
	/// </summary>
	/// <typeparam name="TEntity">Type of the entity.</typeparam>
	public abstract class EntityRepository<TEntity> : RepositoryBase<TEntity>
		where TEntity : class, IEntity
	{
		protected EntityRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		/// <summary>
		///     Maps given instance of <see cref="TDto" /> to a new instance of <see cref="TEntity" /> and saves it to the
		///     database.
		/// </summary>
		/// <typeparam name="TDto">Type of the source data transfer object.</typeparam>
		/// <param name="dto">Data for the new entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected async Task<long> CreateFromDtoAsync<TDto>(TDto dto,
			CancellationToken cancellationToken)
		{
			var entity = Mapper.Map<TEntity>(dto);
			DbSet.Add(entity);
			await SaveChangesAsync(cancellationToken);
			return entity.Id;
		}

		/// <summary>
		///     Gets properties described by <see cref="TDto" /> from the entity with given id.
		/// </summary>
		/// <typeparam name="TDto">Type describing which properties should be loaded from the database.</typeparam>
		/// <param name="id">Id of the entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected Task<TDto> GetDtoByIdAsync<TDto>(long id, CancellationToken cancellationToken)
		{
			return GetDtoByQueryAsync<TDto>(e => e.Id == id, cancellationToken);
		}

		/// <summary>
		///     Updates entity in the database with values from the supplied data transfer object. Returns true if an entity was
		///     updated.
		/// </summary>
		/// <typeparam name="TDto">DTO Type with properties which should be updated.</typeparam>
		/// <param name="id">Id of the entity to be updated.</param>
		/// <param name="dto">Data to update.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected Task<bool> UpdateFromDtoAsync<TDto>(long id, TDto dto,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoByQueryAsync(row => row.Id == id, dto, cancellationToken);
		}

		/// <summary>
		///     Deletes an entity with given id from the database.
		/// </summary>
		/// <param name="id">Id of the entity to delete.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(e => e.Id == id, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			DbSet.Remove(entity);
			await SaveChangesAsync(cancellationToken);
			return true;
		}

		/// <inheritdoc />
		public Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken = default)
		{
			return ExistsByQueryAsync(e => e.Id == id, cancellationToken);
		}

		protected IQueryable<TEntity> QueryById(long id)
		{
			return Query(e => e.Id == id);
		}
	}
}