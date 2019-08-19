using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.Users;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IUserRepository
		: ILookupRepository<UserFilterDto, UserPreviewDto, UserDetailDto>,
			IUpdateRepository<UserProfileDto>
	{
		Task<EmailRecipientDto> FindRecipientAsync(long id, CancellationToken cancellationToken);

		Task<EmailRecipientDto> FindRecipientAsync(string email,
			CancellationToken cancellationToken);
	}
}