using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models.Leaderboards;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/tournaments/{id}/leaderboard")]
	public class LeaderboardController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly ILeaderboardService leaderboardService;

		public LeaderboardController(IAuthorizationService authorizationService,
			ILeaderboardService leaderboardService)
		{
			this.authorizationService = authorizationService;
			this.leaderboardService = leaderboardService;
		}

		/// <summary>
		///     Returns leaderboard of a tournament.
		/// </summary>
		/// <returns>Leaderboard of tournament.</returns>
		/// <response code="200">Tournament data found.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permissions to this resource.</response>
		/// <response code="404">Resource was not found.</response>
		[HttpGet( Name = nameof(GetLeaderboardByTournamentIdAsync))]
		[ProducesResponseType(typeof(LeaderboardModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<LeaderboardModel> GetLeaderboardByTournamentIdAsync(long id,
			CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id,
				TournamentPermission.ViewLeaderboard);
			return await leaderboardService.GetTournamentLeaderboard(id, cancellationToken);
		}
	}
}