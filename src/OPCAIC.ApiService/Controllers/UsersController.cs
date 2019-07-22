using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
		///     Returns lists of users
		/// </summary>
		/// <returns>array of all users</returns>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpGet(Name = nameof(GetUsers))]
		[ProducesResponseType(typeof(UserIdentity), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.Unauthorized)]
		[ProducesResponseType((int)HttpStatusCode.Forbidden)]
		public Task<UserIdentityDto[]> GetUsers(CancellationToken cancellationToken)
		{
			return userService.GetAllAsync(cancellationToken);
		}

		/// <summary>
		///     Generates login token and returns model of current user
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
		///     Creates new user and returns his id
		/// </summary>
		/// <param name="model"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> PostAsync([FromBody] NewUserModel model,
			CancellationToken cancellationToken)
		{
			var dto = mapper.Map<NewUserDto>(model);

			var id = await userService.CreateAsync(dto, cancellationToken);
			return CreatedAtRoute(nameof(GetUsers), new IdModel {Id = id});
		}
	}
}