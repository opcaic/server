using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Interfaces
{
	public interface IUserManager
	{
		Task<User> FindByIdAsync(long id, CancellationToken cancellationToken);
		Task<IdentityResult> CreateAsync(User user, string password);

		Task<ListModel<UserPreviewModel>> GetByFilterAsync(UserFilterModel filter,
			CancellationToken cancellationToken);

		Task<UserDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken);
		Task UpdateAsync(long id, UserProfileModel model, CancellationToken cancellationToken);
		Task<UserTokens> GenerateUserTokensAsync(User user);

		string GetUserName(ClaimsPrincipal principal);
		long GetUserId(ClaimsPrincipal principal);
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