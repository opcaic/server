using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using OPCAIC.Application.Emails.Models;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.Application.Emails.Templates
{
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