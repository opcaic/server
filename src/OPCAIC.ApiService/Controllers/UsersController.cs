using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Infrastructure.Identity;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/users")]
	[Authorize]
	public class UsersController : ControllerBase
	{
		private readonly IEmailService emailService;
		private readonly ILogger<UsersController> logger;
		private readonly SignInManager signInManager;
		private readonly IFrontendUrlGenerator urlGenerator;
		private readonly IUserManager userManager;

		public UsersController(ILogger<UsersController> logger, IEmailService emailService,
			SignInManager signInManager, IUserManager userManager,
			IFrontendUrlGenerator urlGenerator)
		{
			this.logger = logger;
			this.emailService = emailService;
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.urlGenerator = urlGenerator;
		}

		/// <summary>
		///     Returns lists of users
		/// </summary>
		/// <returns>array of all users</returns>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpGet(Name = nameof(GetUsersAsync))]
		[ProducesResponseType(typeof(ListModel<UserPreviewModel>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType((int)HttpStatusCode.Forbidden)]
		[Authorize(RolePolicy.Admin)]
		public Task<ListModel<UserPreviewModel>> GetUsersAsync(UserFilterModel filter,
			CancellationToken cancellationToken)
		{
			return userManager.GetByFilterAsync(filter, cancellationToken);
		}

		/// <summary>
		///     Generates login token and returns model of current user
		/// </summary>
		/// <param name="credentials"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>Model of current user</returns>
		[AllowAnonymous]
		[HttpPost("login")]
		[ProducesResponseType(typeof(UserIdentityModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<UserIdentityModel> LoginAsync([FromBody] UserCredentialsModel credentials,
			CancellationToken cancellationToken)
		{
			var user = await userManager.FindByEmailAsync(credentials.Email);
			if (user != null)
			{
				var result = await signInManager.CheckPasswordSignInAsync(user,
					credentials.Password,
					false);

				if (result.Succeeded)
				{
					var tokens = await userManager.GenerateUserTokensAsync(user);
					logger.LoginSuccess(user);

					return new UserIdentityModel
					{
						Id = user.Id,
						Email = user.Email,
						Role = (UserRole)user.RoleId,
						RefreshToken = tokens.RefreshToken,
						AccessToken = tokens.AccessToken
					};
				}

				if (result.IsNotAllowed)
				{
					logger.LoginNotAllowed(user);
					throw new UnauthorizedException(ValidationErrorCodes.LoginEmailNotConfirmed);
				}

				if (result.IsLockedOut)
				{
					logger.LoginLockout(user);
					throw new UnauthorizedException(ValidationErrorCodes.LoginLockout);
				}

				logger.LoginInvalidPassword(user);
			}
			else
			{
				logger.LoginInvalidMail(credentials.Email);
			}

			throw new UnauthorizedException(ValidationErrorCodes.LoginInvalid);
		}

		/// <summary>
		///     Generates and returns new authorization tokens.
		/// </summary>
		/// <param name="userId">id of signed user</param>
		/// <param name="model">current refresh token</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">returns new authorization tokens</response>
		/// <response code="404">User not found.</response>
		/// <response code="401">refresh token invalid or expired</response>
		[AllowAnonymous]
		[HttpPost("{userId}/refresh")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<UserTokens> RefreshAsync(long userId, [FromBody] RefreshToken model,
			CancellationToken cancellationToken)
		{
			var user = await userManager.FindByIdAsync(userId, cancellationToken);

			if (user == null)
			{
				throw new NotFoundException(nameof(User), userId);
			}

			if (!await userManager.VerifyJwtRefreshToken(user, model.Token))
			{
				throw new UnauthorizedException(ValidationErrorCodes.RefreshTokenInvalid);
			}

			return await userManager.GenerateUserTokensAsync(user);
		}

		/// <summary>
		///     Creates new user and returns his id
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">User created</response>
		/// <response code="400">Data model is invalid.</response>
		[AllowAnonymous]
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> PostAsync([FromBody] NewUserModel model,
			CancellationToken cancellationToken)
		{
			var user = await userManager.CreateAsync(model, cancellationToken);

			var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
			var url = urlGenerator.EmailConfirmLink(user.Id, token);

			await emailService.SendEmailVerificationEmailAsync(user.Id, url,
				cancellationToken);

			logger.UserCreated(user);
			return CreatedAtRoute(nameof(GetUsersAsync), new IdModel {Id = user.Id});
		}

		/// <summary>
		///     Gets user by id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">User data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resouce was not found.</response>
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(UserDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<UserDetailModel> GetUserByIdAsync(long id, CancellationToken cancellationToken)
		{
			// TODO: authorize
			return userManager.GetByIdAsync(id, cancellationToken);
		}

		/// <summary>
		///     Updates user data by id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">User was successfully updated.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpPut("{id}")]
		[ProducesResponseType(typeof(UserDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task UpdateAsync(long id, [FromBody] UserProfileModel model,
			CancellationToken cancellationToken)
		{
			// TODO: authorize
			return userManager.UpdateAsync(id, model, cancellationToken);
		}

		/// <summary>
		///     Creates random key, which can be used to password reset and sends to user's email.
		/// </summary>
		/// <param name="model">email of user, whose password will be reset</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">If user exists, the email reset has been sent.</response>
		[HttpPost("forgotPassword")]
		[AllowAnonymous]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task PostForgotPasswordAsync([FromBody] ForgotPasswordModel model,
			CancellationToken cancellationToken)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null || !user.EmailConfirmed)
			{
				// Do not reveal that the user does not exist or mail not confirmed.
				return;
			}

			logger.UserForgotPassword(user);
			var token = await userManager.GeneratePasswordResetTokenAsync(user);
			var url = urlGenerator.PasswordResetLink(user.Id, token);

			await emailService.SendPasswordResetEmailAsync(model.Email, url, cancellationToken);
		}

		/// <summary>
		///     Resets user's password to a new password using the provided reset token.
		/// </summary>
		/// <param name="model">Values needed to reset the password.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Password was reset successfully</response>
		/// <response code="400">Invalid model values.</response>
		[HttpPost("passwordReset")]
		[AllowAnonymous]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task PostPasswordResetAsync([FromBody] PasswordResetModel model,
			CancellationToken cancellationToken)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				// Do not reveal that the user does not exist or mail not confirmed.
				return;
			}

			var result =
				await userManager.ResetPasswordAsync(user, model.ResetToken, model.Password);
			logger.UserPasswordReset(user, result);
			result.ThrowIfFailed(StatusCodes.Status400BadRequest);
		}

		/// <summary>
		///     Changes user's password, if the old passwords match.
		/// </summary>
		/// <param name="model">email and secret for change and new password</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Password was changed successfully</response>
		/// <response code="400">Invalid model values.</response>
		[HttpPost("password")]
		[AllowAnonymous]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> PostPasswordAsync(
			[FromBody] NewPasswordModel model,
			CancellationToken cancellationToken)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				return BadRequest();
			}

			var result = await userManager.ChangePasswordAsync(user,
				model.OldPassword, model.NewPassword);
			logger.UserPasswordChange(user, result);
			result.ThrowIfFailed(StatusCodes.Status400BadRequest);

			return Ok();
		}

		/// <summary>
		///     Verifies user's email, if he provided valid token created by server.
		/// </summary>
		/// <param name="userId">id of the user, who verifies the mail</param>
		/// <param name="token">verification token created by server</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Email was confirmed.</response>
		/// <response code="400">Invalid model values.</response>
		[HttpGet("emailVerification")]
		[AllowAnonymous]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetEmailVerificationAsync(long userId, string token,
			CancellationToken cancellationToken)
		{
			var user = await userManager.FindByIdAsync(userId, cancellationToken);
			if (user == null || token == null)
			{
				return BadRequest();
			}

			var result = await userManager.ConfirmEmailAsync(user, token);
			result.ThrowIfFailed(StatusCodes.Status400BadRequest);
			logger.UserConfirmEmail(user, result);

			return NoContent();
		}
	}
}