using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IUserRepository
		: ILookupRepository<UserDetailDto>,
			IUpdateRepository<UserProfileDto>,
			IRepository<User>

	{
		Task<EmailRecipientDto> FindRecipientAsync(long id, CancellationToken cancellationToken);

		Task<EmailRecipientDto> FindRecipientAsync(string email,
			CancellationToken cancellationToken);

		Task<List<UserReferenceDto>> GetSubscriberesByTournamentAsync(long tournamentId,
			CancellationToken cancellationToken);
	}
}