using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Broker;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/broker")]
	[Authorize]
	public class BrokerController : ControllerBase
	{
		private readonly IAuthorizationService authorizationService;
		private readonly IBrokerService brokerService;

		public BrokerController(IBrokerService brokerService,
			IAuthorizationService authorizationService)
		{
			this.brokerService = brokerService;
			this.authorizationService = authorizationService;
		}

		/// <summary>
		///     Get a list of work items currently queued on broker, filtered by given filter.
		/// </summary>
		/// <param name="filter">Filter to use.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Request ok.</response>
		/// <response code="400">Data model is invalid.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permission to manage broker.</response>
		/// <returns>Filtered list of current work items.</returns>
		[HttpGet("items")]
		[ProducesResponseType(typeof(List<WorkItemModel>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<List<WorkItemModel>> GetWorkItemsAsync(WorkItemFilterModel filter,
			CancellationToken cancellationToken)
		{
			return brokerService.GetWorkItems(filter, cancellationToken);
		}

		/// <summary>
		///     Cancel a work item with a given id.
		/// </summary>
		/// <param name="id">Id of the work item to cancel.</param>
		/// <response code="200">Request ok.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permission to manage broker.</response>
		[HttpPost("cancellation")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<ResultModel> CancelWorkAsync(Guid id,
			CancellationToken cancellationToken)
		{
			return brokerService.CancelWork(id, cancellationToken);
		}


		/// <summary>
		///     Prioritize a work item queued on broker.
		/// </summary>
		/// <param name="id">Id of work item to prioritize.</param>
		/// <response code="200">Request ok.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permission to manage broker.</response>
		[HttpPost("priority")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<ResultModel> PrioritizeWork(Guid id, CancellationToken cancellationToken)
		{
			return brokerService.PrioritizeWork(id, cancellationToken);
		}

		/// <summary>
		///     Get stats about the current state of the worker.
		/// </summary>
		/// <response code="200">Request ok.</response>
		/// <response code="401">User is not authenticated.</response>
		/// <response code="403">User does not have permission to manage broker.</response>
		[HttpGet]
		[ProducesResponseType(typeof(BrokerStatsModel), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<BrokerStatsModel> GetStatsAsync(CancellationToken cancellationToken)
		{
			return brokerService.GetStats(cancellationToken);
		}
	}
}