using System.Threading.Tasks;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repository
{
	public class SimpleLookupRepository<TEntity>
		: Repository<TEntity>, ISimpleLookupRepository<TEntity> where TEntity : Entity
	{
		/// <inheritdoc />
		protected SimpleLookupRepository(EntityFrameworkDbContext context) : base(context)
		{
		}

		/// <inheritdoc />
		public TEntity Find(long id) => DbSet.Find(id);

		/// <inheritdoc />
		public Task<TEntity> FindAsync(long id) => DbSet.FindAsync(id);

		/// <inheritdoc />
		public void Delete(long id)
		{
			var entity = Find(id);
			if (entity != null)
			{
				Delete(entity);
			}
		}
	}
}
