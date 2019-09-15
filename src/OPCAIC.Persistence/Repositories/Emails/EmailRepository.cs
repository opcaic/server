using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Domain.Entities;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Emails;

namespace OPCAIC.Infrastructure.Repositories.Emails
{
	public class EmailRepository : RepositoryBase<Email>, IEmailRepository
	{
		public EmailRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		public Task<long> EnqueueEmailAsync(NewEmailDto dto,
			CancellationToken cancellationToken)
		{
			return CreateFromDtoAsync(dto, cancellationToken);
		}

		public Task<EmailPreviewDto[]> GetEmailsAsync(CancellationToken cancellationToken)
		{
			return DbSet
				.OrderByDescending(row => row.Created)
				.ProjectTo<EmailPreviewDto>(Mapper.ConfigurationProvider)
				.ToArrayAsync(cancellationToken);
		}

		public Task UpdateResultAsync(long id, EmailResultDto dto,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoAsync(id, dto, cancellationToken);
		}

		public Task<EmailPreviewDto[]> GetEmailsToSendAsync(CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.RemainingAttempts > 0)
				.OrderBy(row => row.Created)
				.ProjectTo<EmailPreviewDto>(Mapper.ConfigurationProvider)
				.ToArrayAsync(cancellationToken);
		}
	}
}