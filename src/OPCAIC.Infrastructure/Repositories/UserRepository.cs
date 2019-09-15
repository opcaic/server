using System.Collections.Generic;
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
		: GenericRepository<User, UserFilterDto, UserPreviewDto, UserDetailDto, NewUserDto,
				UserProfileDto>,
			IUserRepository
	{
		/// <inheritdoc />
		public UserRepository(DataContext context, IMapper mapper)
			: base(context, mapper, QueryableExtensions.Filter)
		{
		}

		public Task<EmailRecipientDto> FindRecipientAsync(long id,
			CancellationToken cancellationToken)
		{
			return QueryById(id)
				.ProjectTo<EmailRecipientDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		public Task<EmailRecipientDto> FindRecipientAsync(string email,
			CancellationToken cancellationToken)
		{
			return Query(row => row.Email == email)
				.ProjectTo<EmailRecipientDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<List<UserReferenceDto>> GetSubscriberesByTournamentAsync(long tournamentId,
			CancellationToken cancellationToken)
		{
			return Query(row
					=> row.WantsEmailNotifications &&
					row.Submissions.Any(s => s.TournamentId == tournamentId))
				.ProjectTo<UserReferenceDto>(Mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}
	}
}