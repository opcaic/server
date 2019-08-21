using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.Services
{
	public class SignInManager : SignInManager<User>
	{

		public new UserManager UserManager => (UserManager)base.UserManager;

		/// <inheritdoc />
		public SignInManager(UserManager<User> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<User> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager> logger, IAuthenticationSchemeProvider schemes) : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes)
		{
		}
	}
}