using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Interfaces.Repositories;

namespace OPCAIC.Persistence.Repositories
{
	/// <summary>
	///     Base class for all repositories containing basic functionality.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public abstract class RepositoryBase<TEntity> : IDisposable
		where TEntity : class
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
		///     Gets the DbSet as an instance of IQueryable
		/// </summary>
		private IQueryable<TEntity> Queryable => DbSet;

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
		protected DbSet<T> GetDbSet<T>() where T : class
		{
			return Context.Set<T>();
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
		/// <param name="query">Query identifying the unique entity to update.</param>
		/// <param name="dto">Data to update.</param>
		/// <param name="cancellationToken"></param>
		protected async Task<bool> UpdateFromDtoByQueryAsync<TDto>(
			Expression<Func<TEntity, bool>> query, TDto dto, CancellationToken cancellationToken)
		{
			// TODO: update without fetching the entity first (or fetch only concurrency stamp)
			var entity = await DbSet.SingleOrDefaultAsync(query, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			Mapper.Map(dto, entity);
			await SaveChangesAsync(cancellationToken);
			return true;
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

		protected IQueryable<TEntity> Query(Expression<Func<TEntity, bool>> predicate)
		{
			return DbSet.Where(predicate);
		}

		protected Task SaveChangesAsync(CancellationToken cancellationToken)
		{
			return Context.SaveChangesAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<TResult> QueryAsync<TResult>(Func<IQueryable<TEntity>, IQueryable<TResult>> query, CancellationToken cancellationToken)
		{
			return query(Queryable).SingleOrDefaultAsync(cancellationToken);
		}
	}
}