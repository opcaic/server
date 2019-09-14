﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Domain.Entities;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.EmailTemplates;

namespace OPCAIC.Infrastructure.Repositories.Emails
{
	public class EmailTemplateRepository : RepositoryBase<EmailTemplate>, IEmailTemplateRepository
	{
		public EmailTemplateRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		public Task<EmailTemplateDto> GetTemplateAsync(string templateName, string lngCode,
			CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Name == templateName && row.LanguageCode == lngCode)
				.ProjectTo<EmailTemplateDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}