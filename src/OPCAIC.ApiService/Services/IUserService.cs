using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Dtos;

namespace OPCAIC.ApiService.Services
{
	public interface IUserService
	{
		Task<long> CreateAsync(NewUserDto user, CancellationToken cancellationToken);

		Task<UserIdentityDto[]> GetAllAsync(CancellationToken cancellationToken);

		Task<UserIdentity> AuthenticateAsync(string email, string passwordHash,
			CancellationToken cancellationToken);
	}
}