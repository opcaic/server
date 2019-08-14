using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Users;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IUserRepository
	{
		Task<long> CreateAsync(NewUserDto user, CancellationToken cancellationToken);

		Task<UserIdentityDto> AuthenticateAsync(string email, string passwordHash,
			CancellationToken cancellationToken);

		Task<ListDto<UserPreviewDto>> GetByFilterAsync(UserFilterDto filter,
			CancellationToken cancellationToken);

		Task<UserDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);
		Task<bool> UpdateAsync(long id, UserProfileDto dto, CancellationToken cancellationToken);
		Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
		Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken);
		Task<UserIdentityDto> FindIdentityAsync(long id, CancellationToken cancellationToken);

		Task<bool> UpdatePasswordKeyAsync(string email, string key,
			CancellationToken cancellationToken);

		Task<UserPasswordDto> FindPasswordDataAsync(string email,
			CancellationToken cancellationToken);

		Task UpdatePasswordDataAsync(long id, UserPasswordDto dto,
			CancellationToken cancellationToken);

		Task<bool> UpdateEmailVerifiedAsync(string email, bool emailVerified,
			CancellationToken cancellationToken);

		Task<EmailRecipientDto> FindRecipientAsync(long id, CancellationToken cancellationToken);

		Task<EmailRecipientDto> FindRecipientAsync(string email,
			CancellationToken cancellationToken);
	}
}