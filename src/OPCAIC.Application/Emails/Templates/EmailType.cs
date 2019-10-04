using System.Collections.Generic;
using System.Linq;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.Application.Emails.Templates
{
	public partial class EmailType : Enumeration<EmailType>
	{
		// enumeration members are placed in separate files

		public class Type<T> : EmailType
			where T : EmailData
		{
			public IEnumerable<string> GetTemplateFields()
			{
				return typeof(T).GetProperties()
					.Select(p => p.Name)
					.Where(p => p != nameof(EmailData.TemplateName));
			}
		}

		public static implicit operator string(EmailType type)
		{
			return type.Name;
		}
	}
}