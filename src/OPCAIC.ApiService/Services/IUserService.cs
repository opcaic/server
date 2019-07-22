using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Dtos;

namespace OPCAIC.ApiService.Services
{
	public interface IUserService
	{
		Task<long> CreateAsync(NewUserDto user, CancellationToken cancellationToken);
		Task<UserIdentityDto[]> GetAllAsync(CancellationToken cancellationToken);
		Task<UserIdentity> AuthenticateAsync(string email, string passwordHash, CancellationToken cancellationToken);
		Task<UserTokens> RefreshTokens(long userId, string oldToken, CancellationToken cancellationToken);
	}
}
