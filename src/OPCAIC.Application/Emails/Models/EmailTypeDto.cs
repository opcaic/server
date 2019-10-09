using System.Collections.Generic;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Infrastructure.AutoMapper;

namespace OPCAIC.Application.Emails.Models
{
	public class EmailTypeDto : IMapFrom<EmailType>
	{
		public string Name { get; set; }

		public IEnumerable<string> TemplateVariables { get; set; }
	}
}