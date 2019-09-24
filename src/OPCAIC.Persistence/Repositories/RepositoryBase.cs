using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Specifications;

namespace OPCAIC.Persistence.Repositories
{
	/// <summary>
	///     Base class for all repositories containing basic functionality.
	/// </summary>
	/// <typeparam name="TEntity"></typeparam>
	public abstract class RepositoryBase<TEntity> : IDisposable, IRepository<TEntity>
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

		/// <inheritdoc />
		public Task<bool> UpdateAsync<TDto>(ISpecification<TEntity> specification, TDto dto, CancellationToken cancellationToken)
		{
			return UpdateFromDtoByQueryAsync(specification.Criteria, dto, cancellationToken);
		}

		/// <inheritdoc />
		public Task<bool> ExistsAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken)
		{
			return Query(specification.Criteria).AnyAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task CreateAsync(TEntity entity, CancellationToken cancellationToken)
		{
			DbSet.Add(entity);
			return SaveChangesAsync(cancellationToken);
		}

		public Task SaveChangesAsync(CancellationToken cancellationToken)
		{
			return Context.SaveChangesAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<List<TEntity>> ListAsync(ISpecification<TEntity> specification,
			CancellationToken cancellationToken)
		{
			return Queryable
				.ApplyOrdering(specification.OrderBy)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<List<TDestination>> ListAsync<TDestination>(
			IProjectingSpecification<TEntity, TDestination> specification,
			CancellationToken cancellationToken)
		{
			return Queryable
				.ApplyOrdering(specification.OrderBy)
				.Select(specification.Projection)
				.ApplyOrdering(specification.OrderByProjected)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public async Task<PagedResult<TEntity>> ListPagedAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken)
		{
			// fetch items and count in one query
			var query = Queryable.ApplyFilter(specification);
			var result = await query
				.ApplyOrdering(specification.OrderBy)
				.ApplyPaging(specification)
				.Select(e => new { Total = query.Count(), Item = e })
				.ToListAsync(cancellationToken);

			if (result.Count > 0)
			{
				return new PagedResult<TEntity>(result[0].Total, result.Select(s => s.Item).ToList());
			}

			return new PagedResult<TEntity>(0, new List<TEntity>());
		}

		/// <inheritdoc />
		public async Task<PagedResult<TDestination>> ListPagedAsync<TDestination>(IProjectingSpecification<TEntity, TDestination> specification,
			CancellationToken cancellationToken)
		{
			// fetch items and count in one query
			var query = Queryable.ApplyFilter(specification);
			var result = await query
				.ApplyOrdering(specification.OrderBy)
				.Select(specification.Projection)
				.ApplyOrdering(specification.OrderByProjected)
				.ApplyPaging(specification)
				.Select(e => new { Total = query.Count(), Item = e })
				.ToListAsync(cancellationToken);

			if (result.Count > 0)
			{
				return new PagedResult<TDestination>(result[0].Total, result.Select(s => s.Item).ToList());
			}

			return new PagedResult<TDestination>(0, new List<TDestination>());
		}

		/// <inheritdoc />
		public Task<TEntity> FindAsync(ISpecification<TEntity> specification,
			CancellationToken cancellationToken)
		{
			return Queryable
				.ApplyFilter(specification)
				.SingleOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<TDestination> FindAsync<TDestination>(
			IProjectingSpecification<TEntity, TDestination> specification,
			CancellationToken cancellationToken)
		{
			return Queryable
				.ApplyFilter(specification)
				.Select(specification.Projection)
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}