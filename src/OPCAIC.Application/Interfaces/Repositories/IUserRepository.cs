using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Users;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IUserRepository
		: IFilterRepository<UserFilterDto, UserPreviewDto>,
			ILookupRepository<UserDetailDto>,
			IUpdateRepository<UserProfileDto>
	{
		Task<EmailRecipientDto> FindRecipientAsync(long id, CancellationToken cancellationToken);

		Task<EmailRecipientDto> FindRecipientAsync(string email,
			CancellationToken cancellationToken);

		Task<List<UserReferenceDto>> GetSubscriberesByTournamentAsync(long tournamentId,
			CancellationToken cancellationToken);
	}
}