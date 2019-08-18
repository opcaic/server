using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/matches")]
	public class MatchController : ControllerBase
	{
		private readonly IMatchService matchService;

		public MatchController(IMatchService matchService)
		{
			this.matchService = matchService;
		}

		/// <summary>
		///     Get filtered list of matches.
		/// </summary>
		/// <param name="filter">Filter to use.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[Authorize(RolePolicy.User)]
		[HttpGet(Name = nameof(GetMatchesAsync))]
		[ProducesResponseType(typeof(ListModel<MatchDetailModel>), (int)HttpStatusCode.OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
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
		[Authorize(RolePolicy.User)]
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(MatchDetailModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<MatchDetailModel> GetMatchById(long id, CancellationToken cancellationToken)
		{
			return matchService.GetByIdAsync(id, cancellationToken);
		}
	}
}