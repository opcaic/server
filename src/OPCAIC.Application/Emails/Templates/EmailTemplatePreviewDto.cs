using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Emails.Templates
{
	public class EmailTemplatePreviewDto : IMapFrom<EmailTemplate>
	{
		public string Name { get; set; }

		public string LanguageCode { get; set; }
	}
}
