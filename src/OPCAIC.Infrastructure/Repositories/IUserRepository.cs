using OPCAIC.Infrastructure.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IUserRepository
	{
		Task<long> CreateAsync(NewUserDto user, CancellationToken cancellationToken);
		Task<UserIdentityDto> AuthenticateAsync(string email, string passwordHash, CancellationToken cancellationToken);
		Task<UserIdentityDto[]> GetAsync(CancellationToken cancellationToken);
		Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
		Task<UserIdentityDto> FindIdentityAsync(long id, CancellationToken cancellationToken);
	}
}