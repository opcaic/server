using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.ApiService.Users.Commands;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Users.Model;
using OPCAIC.Application.Users.Queries;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Infrastructure.Emails;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/users")]
	public class UsersController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IEmailService emailService;
		private readonly ILogger<UsersController> logger;
		private readonly IMapper mapper;
		private readonly SignInManager signInManager;
		private readonly IFrontendUrlGenerator urlGenerator;
		private readonly IUserManager userManager;
		private readonly IMediator mediator;

		public UsersController(ILogger<UsersController> logger, IEmailService emailService,
			SignInManager signInManager, IUserManager userManager,
			IFrontendUrlGenerator urlGenerator, IAuthorizationService authorizationService,
			IMapper mapper, IMediator mediator)
		{
			this.logger = logger;
			this.emailService = emailService;
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.urlGenerator = urlGenerator;
			this.authorizationService = authorizationService;
			this.mapper = mapper;
			this.mediator = mediator;
			this.logger = logger;
		}

		/// <summary>
		///     Returns lists of users
		/// </summary>
		/// <returns>array of all users</returns>
		/// <response code="200"></response>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpGet]
		[ProducesResponseType(typeof(PagedResult<UserPreviewDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(UserPermission.Search)]
		public Task<PagedResult<UserPreviewDto>> GetUsersAsync(GetUsersQuery filter,
			CancellationToken cancellationToken)
		{
			return mediator.Send(filter, cancellationToken);
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
						Role = user.Role,
						RefreshToken = tokens.RefreshToken,
						AccessToken = tokens.AccessToken,
						LocalizationLanguage = user.LocalizationLanguage
					};
				}

				if (result.IsNotAllowed)
				{
					logger.LoginNotAllowed(user);
					throw new UnauthorizedException(null,
						ValidationErrorCodes.LoginEmailNotConfirmed);
				}

				if (result.IsLockedOut)
				{
					logger.LoginLockout(user);
					throw new UnauthorizedException(null, ValidationErrorCodes.LoginLockout);
				}

				logger.LoginInvalidPassword(user);
			}
			else
			{
				logger.LoginInvalidMail(credentials.Email);
			}

			throw new UnauthorizedException(null, ValidationErrorCodes.LoginInvalid);
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
				throw new UnauthorizedException(null, ValidationErrorCodes.RefreshTokenInvalid);
			}

			return await userManager.GenerateUserTokensAsync(user);
		}

		/// <summary>
		///     Creates new user and returns his id.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">User created</response>
		/// <response code="400">Data model is invalid.</response>
		[AllowAnonymous]
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> PostAsync([FromBody] CreateUserCommand model,
			CancellationToken cancellationToken)
		{
			var id = await mediator.Send(model, cancellationToken);
			return CreatedAtRoute(nameof(GetUserByIdAsync), new { id },
				new IdModel { Id = id });
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
		[HttpGet("{id}", Name = nameof(GetUserByIdAsync))]
		[ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<UserDetailDto> GetUserByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, UserPermission.Read);
			return await userManager.GetByIdAsync(id, cancellationToken);
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
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task UpdateAsync(long id, [FromBody] UserProfileModel model,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, UserPermission.Update);
			await userManager.UpdateAsync(id, model, cancellationToken);
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
			var url = urlGenerator.PasswordResetLink(user.Email, token);

			await emailService.EnqueueEmailAsync(new PasswordResetEmailDto(url),
				model.Email, cancellationToken);
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
		public async Task<IActionResult> PostPasswordResetAsync([FromBody] PasswordResetModel model,
			CancellationToken cancellationToken)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null)
			{
				// Do not reveal that the user does not exist or mail not confirmed.
				return BadRequest();
			}

			var result =
				await userManager.ResetPasswordAsync(user, model.ResetToken, model.Password);
			logger.UserPasswordReset(user, result);
			result.ThrowIfFailed();

			return NoContent();
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
		[ProducesResponseType(typeof(UserTokens), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<UserTokens> PostPasswordAsync(
			[FromBody] NewPasswordModel model,
			CancellationToken cancellationToken)
		{
			var user = await userManager.GetUserAsync(User);
			var result = await userManager.ChangePasswordAsync(user,
				model.OldPassword, model.NewPassword);
			logger.UserPasswordChange(user, result);
			result.ThrowIfFailed(nameof(NewPasswordModel.NewPassword));

			return await userManager.GenerateUserTokensAsync(user);
		}

		/// <summary>
		///     Verifies user's email, if he provided valid token created by server.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Email was confirmed.</response>
		/// <response code="400">Invalid model values.</response>
		[HttpPost("emailVerification")]
		[AllowAnonymous]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> GetEmailVerificationAsync(
			[FromBody] EmailVerificationModel model,
			CancellationToken cancellationToken)
		{
			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null || user.EmailConfirmed || model.Token == null)
			{
				return BadRequest();
			}

			var result = await userManager.ConfirmEmailAsync(user, model.Token);
			result.ThrowIfFailed();
			logger.UserConfirmEmail(user, result);

			return NoContent();
		}
	}
}