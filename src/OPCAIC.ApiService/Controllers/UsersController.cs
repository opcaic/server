using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/users")]
	[ApiController]
	[Authorize]
	public class UsersController : ControllerBase
	{
		private readonly IUserService fUserService;

		public UsersController(IUserService userService) => fUserService = userService;

		/// <summary>
		///   Returns lists of users
		/// </summary>
		/// <returns>array of all users</returns>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpGet]
		[ProducesResponseType(typeof(UserIdentity), (int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.Unauthorized)]
		[ProducesResponseType((int) HttpStatusCode.Forbidden)]
		public async Task<ActionResult<UserIdentity[]>> GetUsers()
			=> Ok(await fUserService.GetAllAsync());

		/// <summary>
		///   Generates login token and returns model of current user
		/// </summary>
		/// <param name="credentials"></param>
		/// <returns>Model of current user</returns>
		[AllowAnonymous]
		[HttpPost("login")]
		[ProducesResponseType(typeof(UserIdentity), (int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.Unauthorized)]
		public async Task<ActionResult<UserIdentity>> LoginAsync([FromBody] UserCredentials credentials)
		{
			var user = await fUserService.Authenticate(credentials.Email, credentials.PasswordHash);
			if (user == null)
			{
				return Unauthorized();
			}

			return Ok(user);
		}
	}
}
