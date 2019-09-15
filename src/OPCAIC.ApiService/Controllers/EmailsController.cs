using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Dtos.Emails;
using OPCAIC.Application.Interfaces.Repositories;

namespace OPCAIC.ApiService.Controllers
{
	[Authorize]
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
		[RequiresPermission(EmailPermission.Read)]
		public Task<EmailPreviewDto[]> GetEmailsAsync(CancellationToken cancellationToken)
		{
			return emailRepository.GetEmailsAsync(cancellationToken);
		}
	}
}