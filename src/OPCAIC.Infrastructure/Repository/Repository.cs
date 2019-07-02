using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repository
{
	public abstract class Repository<TEntity> : IDisposable, IRepository<TEntity>
		where TEntity : Entity
	{
		protected Repository(EntityFrameworkDbContext context) => Context = context;

		/// <summary>
		///   Underlying EF database context.
		/// </summary>
		protected EntityFrameworkDbContext Context { get; }

		/// <summary>
		///   Database set containing the entities.
		/// </summary>
		protected DbSet<TEntity> DbSet => Context.Set<TEntity>();

		/// <inheritdoc />
		public void Dispose() => Context?.Dispose();

		/// <inheritdoc />
		public void Delete(TEntity entity) => DbSet.Remove(entity);

		/// <inheritdoc />
		public void Add(TEntity entity) => DbSet.Add(entity);

		/// <inheritdoc />
		public void SaveChanges() => Context.SaveChanges();

		/// <inheritdoc />
		public Task SaveChangesAsync() => Context.SaveChangesAsync();
	}
}
