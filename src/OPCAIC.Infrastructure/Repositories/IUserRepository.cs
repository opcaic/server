using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Users;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IUserRepository
	{
		Task<ListDto<UserPreviewDto>> GetByFilterAsync(UserFilterDto filter,
			CancellationToken cancellationToken);

		Task<bool> UpdateAsync(long id, UserProfileDto dto, CancellationToken cancellationToken);

		Task<EmailRecipientDto> FindRecipientAsync(long id, CancellationToken cancellationToken);

		Task<EmailRecipientDto> FindRecipientAsync(string email,
			CancellationToken cancellationToken);
	}
}