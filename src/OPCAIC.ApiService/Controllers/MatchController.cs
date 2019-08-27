using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/matches")]
	public class MatchController : ControllerBase
	{
		private readonly IMatchService matchService;
		private readonly IAuthorizationService authorizationService;

		public MatchController(IMatchService matchService, IAuthorizationService authorizationService)
		{
			this.matchService = matchService;
			this.authorizationService = authorizationService;
		}

		/// <summary>
		///     Get filtered list of matches.
		/// </summary>
		/// <param name="filter">Filter to use.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet]
		[ProducesResponseType(typeof(ListModel<MatchDetailModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[RequiresPermission(MatchPermission.Search)]
		public Task<ListModel<MatchDetailModel>> GetMatchesAsync(MatchFilterModel filter,
			CancellationToken cancellationToken)
		{
			return matchService.GetByFilterAsync(filter, cancellationToken);
		}

		/// <summary>
		///     Gets match by id.
		/// </summary>
		/// <param name="id">Id of match to look for.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(MatchDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<MatchDetailModel> GetMatchById(long id, CancellationToken cancellationToken)
		{
			await authorizationService.CheckPermissions(User, id, MatchPermission.Read);
			return await matchService.GetByIdAsync(id, cancellationToken);
		}
	}
}