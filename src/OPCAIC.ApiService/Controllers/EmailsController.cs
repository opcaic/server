using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Emails.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Extensions;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
	[Route("api/emails")]
	public class EmailsController : ControllerBase
	{
		private readonly IMapper mapper;
		private readonly IRepository<Email> emailRepository;

		public EmailsController(IRepository<Email> emailRepository, IMapper mapper)
		{
			this.emailRepository = emailRepository;
			this.mapper = mapper;
		}

		/// <summary>
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet]
		[RequiresPermission(EmailPermission.Read)]
		public Task<List<EmailDto>> GetEmailsAsync(CancellationToken cancellationToken)
		{
			return emailRepository.ListAsync<Email, EmailDto>(e => true, mapper, cancellationToken);
		}
	}
}