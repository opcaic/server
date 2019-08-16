using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public abstract class Repository<TEntity> : IDisposable, IRepository<TEntity>
		where TEntity : EntityBase
	{
		protected Repository(DataContext context, IMapper mapper)
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
		protected DbSet<TEntity> DbSet => Context.Set<TEntity>();

		protected IMapper Mapper { get; }

		/// <inheritdoc />
		public void Dispose()
		{
			Context?.Dispose();
		}

		/// <inheritdoc />
		public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
		{
			return DbSet.AnyAsync(e => e.Id == id, cancellationToken);
		}

		/// <inheritdoc />
		public bool Exists(long id)
		{
			return DbSet.Any(e => e.Id == id);
		}

		/// <inheritdoc />
		public void Delete(TEntity entity)
		{
			DbSet.Remove(entity);
		}

		/// <inheritdoc />
		public void Add(TEntity entity)
		{
			DbSet.Add(entity);
		}

		/// <inheritdoc />
		public void SaveChanges()
		{
			Context.SaveChanges();
		}

		/// <inheritdoc />
		public Task SaveChangesAsync()
		{
			return Context.SaveChangesAsync();
		}
	}
}