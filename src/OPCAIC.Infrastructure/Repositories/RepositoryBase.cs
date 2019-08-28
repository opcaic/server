using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	/// <summary>
	///     Base class for repository of <see cref="TEntity" /> instances.
	/// </summary>
	/// <typeparam name="TEntity">Type of the entity.</typeparam>
	public abstract class RepositoryBase<TEntity> : IDisposable
		where TEntity : class, IEntity
	{
		protected RepositoryBase(DataContext context, IMapper mapper)
		{
			Context = context;
			Mapper = mapper;
		}

		/// <summary>
		///     Underlying EF database context.
		/// </summary>
		protected DataContext Context { get; }

		/// <summary>
		///     Database set containing the entities.
		/// </summary>
		protected DbSet<TEntity> DbSet => GetDbSet<TEntity>();

		/// <summary>
		///     Instance of <see cref="IMapper" /> to use for mapping between objects.
		/// </summary>
		protected IMapper Mapper { get; }

		/// <inheritdoc />
		public void Dispose()
		{
			Context?.Dispose();
		}

		/// <summary>
		///     Database set containing the entities of type <see cref="T" />.
		/// </summary>
		protected DbSet<T> GetDbSet<T>() where T : class, IEntity
		{
			return Context.Set<T>();
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
		///     Gets properties described by <see cref="TDto" /> from the entity satisfying given query.
		/// </summary>
		/// <typeparam name="TDto">Type describing which properties should be loaded from the database.</typeparam>
		/// <param name="predicate"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected Task<TDto> GetDtoByQueryAsync<TDto>(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken)
		{
			return Query(predicate)
				.ProjectTo<TDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
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
		protected async Task<bool> UpdateFromDtoAsync<TDto>(long id, TDto dto,
			CancellationToken cancellationToken)
		{
			// TODO: update without fetching the entity first (or fetch only concurrency stamp)
			var entity = await DbSet.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			Mapper.Map(dto, entity);
			await SaveChangesAsync(cancellationToken);
			return true;
		}

		/// <summary>
		///     Deletes an entity with given id from the database.
		/// </summary>
		/// <param name="id">Id of the entity to delete.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(e => e.Id == id, cancellationToken: cancellationToken);
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

		/// <summary>
		///     Returns true, if there exists an entity in the DB which satisfies given predicate.
		/// </summary>
		/// <param name="predicate">Predicate to be tested.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		protected Task<bool> ExistsByQueryAsync(Expression<Func<TEntity, bool>> predicate,
			CancellationToken cancellationToken = default)
		{
			return DbSet.AnyAsync(predicate, cancellationToken);
		}

		protected IQueryable<TEntity> QueryById(long id)
		{
			return Query(e => e.Id == id);
		}

		protected IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate)
		{
			return DbSet.Where(predicate);
		}

		protected Task SaveChangesAsync(CancellationToken cancellationToken)
		{
			return Context.SaveChangesAsync(cancellationToken);
		}
	}
}