using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.Application.Dtos.Base;
using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public class SubmissionRepository
		: GenericRepository<Submission, SubmissionDetailDto, NewSubmissionDto, UpdateSubmissionScoreDto>,
			ISubmissionRepository
	{
		/// <inheritdoc />
		public SubmissionRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<SubmissionDtoBase> FindSubmissionForStorageAsync(long id,
			CancellationToken cancellationToken = default)
		{
			return GetDtoByIdAsync<SubmissionDtoBase>(id, cancellationToken);
		}
	}
}