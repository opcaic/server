using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class UserRepository
		: LookupRepository<User, UserFilterDto, UserPreviewDto, UserDetailDto>, IUserRepository
	{
		/// <inheritdoc />
		public UserRepository(DataContext context, IMapper mapper)
			: base(context, mapper, QueryableExtensions.Filter)
		{
		}

		public Task<bool> UpdateAsync(long id, UserProfileDto dto,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoAsync(id, dto, cancellationToken);
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