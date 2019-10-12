using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Services
{
	public class SignInManager : SignInManager<User>
	{
		/// <inheritdoc />
		public SignInManager(UserManager<User> userManager, IHttpContextAccessor contextAccessor,
			IUserClaimsPrincipalFactory<User> claimsFactory,
			IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<User>> logger,
			IAuthenticationSchemeProvider schemes, IUserConfirmation<User> confirmation) : base(
			userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes,
			confirmation)
		{
		}

		public new UserManager UserManager => (UserManager)base.UserManager;
	}
}