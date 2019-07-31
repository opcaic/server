using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models; 
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;

namespace OPCAIC.ApiService.Services
{
	public interface IUserService
	{
		Task<long> CreateAsync(NewUserModel user, CancellationToken cancellationToken);
		Task<ListModel<UserPreviewModel>> GetByFilterAsync(UserFilterModel filter, CancellationToken cancellationToken);
		Task<UserDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken);
		Task UpdateAsync(long id, UserProfileModel model, CancellationToken cancellationToken);
		Task<UserIdentityModel> AuthenticateAsync(string email, string passwordHash, CancellationToken cancellationToken);
		Task<UserTokens> RefreshTokens(long userId, string oldToken, CancellationToken cancellationToken);
	}
}