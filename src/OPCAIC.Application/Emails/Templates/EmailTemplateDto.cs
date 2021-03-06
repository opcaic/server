﻿using System.Collections.Generic;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Emails.Templates
{
	public class EmailTemplateDto : IMapFrom<EmailTemplate>
	{
		public string Name { get; set; }
		
		public string LanguageCode { get; set; }

		[IgnoreMap]
		public IEnumerable<string> TemplateVariables { get; set; }

		public string SubjectTemplate { get; set; }

		public string BodyTemplate { get; set; }
	}
}