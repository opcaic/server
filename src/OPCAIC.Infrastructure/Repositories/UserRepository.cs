using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class UserRepository : Repository<User>, IUserRepository
	{
		public UserRepository(DataContext dataContext, IMapper mapper)
			: base(dataContext, mapper)
		{
		}

		public async Task<ListDto<UserPreviewDto>> GetByFilterAsync(UserFilterDto filter,
			CancellationToken cancellationToken)
		{
			var query = DbSet.Filter(filter);

			return new ListDto<UserPreviewDto>
			{
				List = await query
					.Skip(filter.Offset)
					.Take(filter.Count)
					.ProjectTo<UserPreviewDto>(Mapper.ConfigurationProvider)
					.ToListAsync(cancellationToken),
				Total = await query.CountAsync(cancellationToken)
			};
		}

		public async Task<long> CreateAsync(NewUserDto user, CancellationToken cancellationToken)
		{
			var entity = new User
			{
				Email = user.Email,
				Username = user.Username,
				Organization = user.Organization,
				LocalizationLanguage = user.LocalizationLanguage,
				PasswordHash = user.PasswordHash,
				RoleId = user.RoleId,
				EmailVerified = false
			};

			DbSet.Add(entity);

			await Context.SaveChangesAsync(cancellationToken);

			return entity.Id;
		}

		public Task<UserIdentityDto> AuthenticateAsync(string email, string passwordHash,
			CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Email == email && row.PasswordHash == passwordHash)
				.ProjectTo<UserIdentityDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		public async Task<UserDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return await DbSet
				.Where(row => row.Id == id)
				.ProjectTo<UserDetailDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		public async Task<bool> UpdateAsync(long id, UserProfileDto dto,
			CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			entity.Organization = dto.Organization;
			entity.LocalizationLanguage = dto.LocalizationLanguage;

			await Context.SaveChangesAsync(cancellationToken);
			return true;
		}

		public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Email == email)
				.AnyAsync(cancellationToken);
		}

		public Task<bool> ExistsByUsernameAsync(string username,
			CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Username == username)
				.AnyAsync(cancellationToken);
		}

		public Task<UserIdentityDto> FindIdentityAsync(long id, CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Id == id)
				.ProjectTo<UserIdentityDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		public async Task<bool> UpdatePasswordKeyAsync(string email, string key,
			CancellationToken cancellationToken)
		{
			var entity =
				await DbSet.SingleOrDefaultAsync(row => row.Email == email, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			entity.PasswordKey = key;
			await Context.SaveChangesAsync(cancellationToken);
			return true;
		}

		public Task<UserPasswordDto> FindPasswordDataAsync(string email,
			CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Email == email)
				.ProjectTo<UserPasswordDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		public async Task UpdatePasswordDataAsync(long id, UserPasswordDto dto,
			CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);

			entity.PasswordHash = dto.PasswordHash;
			entity.PasswordKey = dto.PasswordKey;
			await Context.SaveChangesAsync(cancellationToken);
		}

		public async Task<bool> UpdateEmailVerifiedAsync(string email, bool emailVerified,
			CancellationToken cancellationToken)
		{
			var entity =
				await DbSet.SingleOrDefaultAsync(row => row.Email == email, cancellationToken);
			if (entity == null)
			{
				return false;
			}

			entity.EmailVerified = emailVerified;
			await Context.SaveChangesAsync(cancellationToken);
			return true;
		}

		public Task<EmailRecipientDto> FindRecipientAsync(long id,
			CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Id == id)
				.ProjectTo<EmailRecipientDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		public Task<EmailRecipientDto> FindRecipientAsync(string email,
			CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Email == email)
				.ProjectTo<EmailRecipientDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}