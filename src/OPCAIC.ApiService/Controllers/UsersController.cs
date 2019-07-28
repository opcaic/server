using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/users")]
	[Authorize]
	public class UsersController : ControllerBase
	{
		private readonly IUserService userService;

		public UsersController(IUserService userService)
		{
			this.userService = userService;
		}

		/// <summary>
		///   Returns lists of users
		/// </summary>
		/// <returns>array of all users</returns>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpGet(Name = nameof(GetUsersAsync))]
		[ProducesResponseType(typeof(ListModel<UserPreviewModel>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType((int)HttpStatusCode.Forbidden)]
		[Authorize(RolePolicy._Admin)]
		public Task<ListModel<UserPreviewModel>> GetUsersAsync(UserFilterModel filter, CancellationToken cancellationToken)
		{
			return userService.GetByFilterAsync(filter, cancellationToken);
		}

		/// <summary>
		///   Generates login token and returns model of current user
		/// </summary>
		/// <param name="credentials"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>Model of current user</returns>
		[AllowAnonymous]
		[HttpPost("login")]
		[ProducesResponseType(typeof(UserIdentityModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public async Task<UserIdentityModel> LoginAsync([FromBody] UserCredentialsModel credentials, CancellationToken cancellationToken)
		{
			var user = await userService.AuthenticateAsync(credentials.Email,
				Hashing.HashPassword(credentials.Password), cancellationToken);
			if (user == null)
			{
				throw new UnauthorizedExcepion("Invalid username or password.");
			}

			return user;
		}

		/// <summary>
		///		Generates and returns new authorization tokens.
		/// </summary>
		/// <param name="userId">id of signed user</param>
		/// <param name="model">current refresh token</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">returns new authorization tokens</response>
		/// <response code="401">refresh token invalid or expired</response>
		[AllowAnonymous]
		[HttpPost("{userId}/refresh")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public Task<UserTokens> RefreshAsync(long userId, [FromBody] RefreshToken model, CancellationToken cancellationToken)
		{
			return userService.RefreshTokens(userId, model.Token, cancellationToken);
		}

		/// <summary>
		///  Creates new user and returns his id
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="201">User created</response>
		/// <response code="400">Data model is invalid.</response>
		[AllowAnonymous]
		[HttpPost]
		[ProducesResponseType(typeof(IdModel), StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> PostAsync([FromBody] NewUserModel model, CancellationToken cancellationToken)
		{
			long id = await userService.CreateAsync(model, cancellationToken);
			return CreatedAtRoute(nameof(GetUsersAsync), new IdModel { Id = id });
		}

		/// <summary>
		///		Gets user by id.
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
			return userService.GetByIdAsync(id, cancellationToken);
		}

		/// <summary>
		///		Updates user data by id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">User was succesfully updated.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resouce was not found.</response>
		[HttpPut("{id}")]
		[ProducesResponseType(typeof(UserDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task	UpdateAsync(long id, [FromBody] UserProfileModel model, CancellationToken cancellationToken)
		{
			return userService.UpdateAsync(id, model, cancellationToken);
		}
	}
}