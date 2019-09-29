using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public class UserRepository
		: GenericRepository<User, UserDetailDto, NewUserDto, UserProfileDto>,
			IUserRepository
	{
		/// <inheritdoc />
		public UserRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
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