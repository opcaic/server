using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.Services
{
	public interface IUserManager
	{
		Task<User> FindByIdAsync(long id, CancellationToken cancellationToken);
		Task<User> CreateAsync(NewUserModel userModel, CancellationToken cancellationToken);

		Task<ListModel<UserPreviewModel>> GetByFilterAsync(UserFilterModel filter,
			CancellationToken cancellationToken);

		Task<UserDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken);
		Task UpdateAsync(long id, UserProfileModel model, CancellationToken cancellationToken);
		Task<UserTokens> GenerateUserTokensAsync(User user);

		string GetUserName(ClaimsPrincipal principal);
		string GetUserId(ClaimsPrincipal principal);
		Task<User> GetUserAsync(ClaimsPrincipal principal);
		Task<User> FindByNameAsync(string userName);
		Task<string> GetUserNameAsync(User user);
		Task<string> GetUserIdAsync(User user);

		Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword,
			string newPassword);

		Task<string> GeneratePasswordResetTokenAsync(User user);
		Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
		Task<User> FindByLoginAsync(string loginProvider, string providerKey);
		Task<User> FindByEmailAsync(string email);
		Task<string> GenerateEmailConfirmationTokenAsync(User user);
		Task<IdentityResult> ConfirmEmailAsync(User user, string token);
		Task<bool> VerifyJwtRefreshToken(User user, string token);
	}
}