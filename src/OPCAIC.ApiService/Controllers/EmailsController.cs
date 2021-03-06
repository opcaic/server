﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.ModelBinding;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Emails.Commands;
using OPCAIC.Application.Emails.Models;
using OPCAIC.Application.Emails.Queries;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Infrastructure;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/emails")]
	public class EmailsController : ControllerBase
	{
		private readonly IMediator mediator;

		public EmailsController(IMediator mediator)
		{
			this.mediator = mediator;
		}

		/// <summary>
		///     Gets emails filtered by query parameters.
		/// </summary>
		/// <param name="query">Query parameters</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200"></response>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpGet]
		[RequiresPermission(EmailPermission.Read)]
		[ProducesResponseType(typeof(PagedResult<EmailDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<PagedResult<EmailDto>> GetEmailsAsync([FromQuery] GetEmailsQuery query,
			CancellationToken cancellationToken)
		{
			return mediator.Send(query, cancellationToken);
		}

		/// <summary>
		///     Gets email templates from the platform.
		/// </summary>
		/// <param name="query">Query parameters.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Email templates returned.</response>
		/// <response code="400">Invalid input model.</response>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpGet("templates")]
		[RequiresPermission(EmailPermission.ManageTemplates)]
		[ProducesResponseType(typeof(PagedResult<EmailTemplatePreviewDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<PagedResult<EmailTemplatePreviewDto>> GetEmailTemplatesAsync(
			[FromQuery] GetEmailTemplatesQuery query, CancellationToken cancellationToken)
		{
			return mediator.Send(query, cancellationToken);
		}

		/// <summary>
		///     Gets email templates from the platform.
		/// </summary>
		/// <param name="query">Query parameters.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <response code="200">Email templates returned.</response>
		/// <response code="400">Invalid input model.</response>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpGet("templates/{Name}/{LanguageCode}")]
		[RequiresPermission(EmailPermission.ManageTemplates)]
		[ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task<EmailTemplateDto> GetEmailTemplateAsync(
			[FromRoute] GetEmailTemplateQuery query, CancellationToken cancellationToken)
		{
			return mediator.Send(query, cancellationToken);
		}

		/// <summary>
		///     Sets values for email template for given email type and language code.
		/// </summary>
		/// <param name="model">The actual email template.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Email template successfully set.</response>
		/// <response code="400">Invalid input model.</response>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpPut("templates/{name}/{languageCode}")]
		[RequiresPermission(EmailPermission.ManageTemplates)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task SetTemplateAsync([FromRouteAndBody] SetEmailTemplateCommand model,
			CancellationToken cancellationToken)
		{
			return mediator.Send(model, cancellationToken);
		}

		/// <summary>
		///     Deletes email template for given email type and language code.
		/// </summary>
		/// <param name="model">The actual email template.</param>
		/// <param name="cancellationToken"></param>
		/// <response code="200">Email template successfully deleted.</response>
		/// <response code="400">Invalid input model.</response>
		/// <response code="401">User is not authorized.</response>
		/// <response code="403">User does not have permission to this action.</response>
		[HttpDelete("templates/{Name}/{LanguageCode}")]
		[RequiresPermission(EmailPermission.ManageTemplates)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		public Task DeleteTemplateAsync([FromRoute] DeleteEmailTemplateCommand model,
			CancellationToken cancellationToken)
		{
			return mediator.Send(model, cancellationToken);
		}
	}
}