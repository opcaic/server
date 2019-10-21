using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.Application.Emails.Templates
{
	[TypeConverter(typeof(EnumerationConverter<EmailType>))]
	public partial class EmailType : Enumeration<EmailType>
	{
		public IEnumerable<string> TemplateVariables { get; }

		// enumeration members are placed in separate files
		public EmailType()
		{
			// only to satisfy constraint for Enumeration<EmailType>
			throw new InvalidOperationException("Should never be called, EmailType(Type) instead");
		}

		public EmailType(Type dataType)
		{
			TemplateVariables = GetTemplateVariables(dataType);
		}

		private static IEnumerable<string> GetTemplateVariables(Type dataType)
		{
			return dataType.GetProperties()
				.Select(p => p.Name)
				.Where(p => p != nameof(EmailData.TemplateName)).ToList();
		}

		public static implicit operator string(EmailType type)
		{
			return type.Name;
		}
	}
}