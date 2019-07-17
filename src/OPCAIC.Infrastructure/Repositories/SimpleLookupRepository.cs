using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class SimpleLookupRepository<TEntity>
		: Repository<TEntity>, ISimpleLookupRepository<TEntity> where TEntity : Entity
	{
		/// <inheritdoc />
		protected SimpleLookupRepository(DataContext context, IMapper mapper) 
      : base(context, mapper)
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
