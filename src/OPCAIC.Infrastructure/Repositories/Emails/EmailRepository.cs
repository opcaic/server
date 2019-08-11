using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Emails;
using OPCAIC.Infrastructure.Entities;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories.Emails
{
	public class EmailRepository: Repository<Email>, IEmailRepository
	{
		public EmailRepository(DataContext context, IMapper mapper)
			:base(context, mapper) { }

		public async Task<long> EnqueueEmailAsync(NewEmailDto dto, CancellationToken cancellationToken)
		{
			var entity = new Email
			{
				RecipientEmail = dto.RecipientEmail,
				TemplateName = dto.TemplateName,
				Subject = dto.Subject,
				Body = dto.Body,
				RemainingAttempts = 3
			};

			DbSet.Add(entity);

			await Context.SaveChangesAsync(cancellationToken);

			return entity.Id;
		}

		public async Task UpdateResultAsync(long id, EmailResultDto dto, CancellationToken cancellationToken)
		{
			var entity = await DbSet.SingleOrDefaultAsync(row => row.Id == id, cancellationToken);

			entity.LastException = dto.LastException.Message;
			entity.SentAt = dto.SentAt;
			entity.RemainingAttempts = dto.RemainingAttempts;

			await Context.SaveChangesAsync(cancellationToken);
		}

		public Task<EmailPreviewDto[]> GetEmailsAsync(CancellationToken cancellationToken)
		{
			return DbSet
				.OrderByDescending(row => row.Created)
				.ProjectTo<EmailPreviewDto>(Mapper.ConfigurationProvider)
				.ToArrayAsync(cancellationToken);
		}
	}
}
