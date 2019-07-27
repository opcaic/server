using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class UserRepository : Repository<User>, IUserRepository
	{
		public UserRepository(DataContext dataContext, IMapper mapper)
		  : base(dataContext, mapper) { }

		public async Task<long> CreateAsync(NewUserDto user, CancellationToken cancellationToken)
		{
			User entity = new User
			{
				Email = user.Email,
				FirstName = user.FirstName,
				LastName = user.LastName,
				PasswordHash = user.PasswordHash,
				RoleId = user.RoleId,
				EmailVerified = false,
			};

			DbSet.Add(entity);

			await Context.SaveChangesAsync(cancellationToken);

			return entity.Id;
		}

		public Task<UserIdentityDto> AuthenticateAsync(string email, string passwordHash, CancellationToken cancellationToken)
		{
			return DbSet
			  .Where(row => row.Email == email && row.PasswordHash == passwordHash)
			  .ProjectTo<UserIdentityDto>(Mapper.ConfigurationProvider)
			  .SingleOrDefaultAsync(cancellationToken);
		}

		public Task<UserIdentityDto[]> GetAsync(CancellationToken cancellationToken)
		{
			return DbSet
			  .ProjectTo<UserIdentityDto>(Mapper.ConfigurationProvider)
			  .ToArrayAsync(cancellationToken);
		}

		public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
		{
			return DbSet
			  .Where(row => row.Email == email)
			  .AnyAsync(cancellationToken);
		}

		public Task<UserIdentityDto> FindIdentityAsync(long id, CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Id == id)
				.ProjectTo<UserIdentityDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}
	}

}
