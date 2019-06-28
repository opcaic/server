using System;
using System.Xml.Xsl;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repository
{
	public interface IRepository<TEntity> where TEntity : Entity
	{
		TEntity Find(long id);
		void Delete(long id);

		void SaveChanges();
	}

	public interface ITournamentRepository : IRepository<Tournament>
	{
	}

	public interface IMatchRepository : IRepository<Match>
	{
	}

	public abstract class RepositoryBase<TEntity> : IDisposable, IRepository<TEntity> where TEntity : Entity
	{
		protected EntityFrameworkDbContext Context { get; }

		protected DbSet<TEntity> DbSet => Context.Set<TEntity>();

		protected RepositoryBase(EntityFrameworkDbContext context)
		{
			this.Context = context;
		}

		/// <inheritdoc />
		public TEntity Find(long id)
		{
			return DbSet.Find(id);
		}

		/// <inheritdoc />
		public void Delete(long id)
		{
		}

		/// <inheritdoc />
		public void SaveChanges()
		{
			Context.SaveChanges();
		}

		/// <inheritdoc />
		public void Dispose() => Context?.Dispose();
	}
}
