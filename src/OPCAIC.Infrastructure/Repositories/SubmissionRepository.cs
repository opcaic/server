using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class SubmissionRepository : RepositoryBase<Submission>, ISubmissionRepository
	{
		/// <inheritdoc />
		public SubmissionRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<SubmissionStorageDto> FindSubmissionForStorageAsync(long id,
			CancellationToken cancellationToken = default)
		{
			return DbSet.Where(s => s.Id == id)
				.ProjectTo<SubmissionStorageDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}