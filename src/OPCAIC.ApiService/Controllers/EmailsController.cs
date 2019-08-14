﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.Infrastructure.Dtos.Emails;
using OPCAIC.Infrastructure.Repositories.Emails;

namespace OPCAIC.ApiService.Controllers
{
	[Route("api/emails")]
	public class EmailsController : ControllerBase
	{
		private readonly IEmailRepository emailRepository;

		public EmailsController(IEmailRepository emailRepository)
		{
			this.emailRepository = emailRepository;
		}

		/// <summary>
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet]
		public Task<EmailPreviewDto[]> GetEmailsAsync(CancellationToken cancellationToken)
		{
			return emailRepository.GetEmailsAsync(cancellationToken);
		}
	}
}