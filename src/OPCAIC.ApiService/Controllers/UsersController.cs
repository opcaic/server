using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Dtos;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/users")]
	[Authorize]
	public class UsersController : ControllerBase
	{
		private readonly IMapper mapper;
		private readonly IUserService userService;

		public UsersController(
			IMapper mapper,
			IUserService userService)
		{
			this.mapper = mapper;
			this.userService = userService;
		}

		/// <summary>
		///   Returns lists of users
		/// </summary>
		/// <returns>array of all users</returns>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpGet(Name = nameof(GetUsers))]
		[ProducesResponseType(typeof(UserIdentity), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType((int)HttpStatusCode.Forbidden)]
		[Authorize(RolePolicy.Admin)]
		public Task<UserIdentityDto[]> GetUsers(CancellationToken cancellationToken)
			=> userService.GetAllAsync(cancellationToken);

		/// <summary>
		///   Generates login token and returns model of current user
		/// </summary>
		/// <param name="credentials"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>Model of current user</returns>
		[AllowAnonymous]
		[HttpPost("login")]
		[ProducesResponseType(typeof(UserIdentity), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		public async Task<UserIdentity> LoginAsync([FromBody] UserCredentialsModel credentials,
			CancellationToken cancellationToken)
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
		public Task<UserTokens> RefreshAsync(long userId, [FromBody] RefreshToken model,
			CancellationToken cancellationToken)
			=> userService.RefreshTokens(userId, model.Token, cancellationToken);

		/// <summary>
		///  Creates new user and returns his id
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpPost]
		public async Task<IActionResult> PostAsync([FromBody] NewUserModel model,
			CancellationToken cancellationToken)
		{
			var dto = mapper.Map<NewUserDto>(model);

			var id = await userService.CreateAsync(dto, cancellationToken);
			return CreatedAtRoute(nameof(GetUsers), new IdModel {Id = id});
		}
	}
}