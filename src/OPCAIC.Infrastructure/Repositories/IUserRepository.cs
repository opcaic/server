using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Users;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IUserRepository
	{
		Task<long> CreateAsync(NewUserDto user, CancellationToken cancellationToken);
		Task<UserIdentityDto> AuthenticateAsync(string email, string passwordHash, CancellationToken cancellationToken);
		Task<ListDto<UserPreviewDto>> GetByFilterAsync(UserFilterDto filter, CancellationToken cancellationToken);
		Task<UserDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);
		Task<bool> UpdateAsync(long id, UserProfileDto dto, CancellationToken cancellationToken);
		Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
		Task<UserIdentityDto> FindIdentityAsync(long id, CancellationToken cancellationToken);
	}
}