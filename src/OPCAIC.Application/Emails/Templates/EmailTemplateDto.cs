using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Emails.Templates
{
	public class EmailTemplateDto : IMapFrom<EmailTemplate>
	{
		public string SubjectTemplate { get; set; }

		public string BodyTemplate { get; set; }
	}
}