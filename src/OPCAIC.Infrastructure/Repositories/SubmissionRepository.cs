using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class SubmissionRepository
		: GenericRepository<Submission, SubmissionFilterDto, SubmissionPreviewDto,
				SubmissionDetailDto, NewSubmissionDto, UpdateSubmissionDto>,
			ISubmissionRepository
	{
		/// <inheritdoc />
		public SubmissionRepository(DataContext context, IMapper mapper)
			: base(context, mapper, QueryableExtensions.Filter)
		{
		}

		/// <inheritdoc />
		public Task<SubmissionStorageDto> FindSubmissionForStorageAsync(long id,
			CancellationToken cancellationToken = default)
		{
			return GetDtoByIdAsync<SubmissionStorageDto>(id, cancellationToken);
		}

		/// <inheritdoc />
		public Task<List<SubmissionDetailDto>> AllSubmissionsFromTournament(long tournamentId,
			CancellationToken cancellationToken)
		{
			return Query(s => s.TournamentId == tournamentId)
				.ProjectTo<SubmissionDetailDto>(Mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<bool> UpdateSubmissionScoreAsync(long id, UpdateSubmissionScoreDto update,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoAsync(id, update, cancellationToken);
		}

		/// <inheritdoc />
		public Task<SubmissionAuthDto> GetAuthorizationData(long id,
			CancellationToken cancellationToken = default)
		{
			return GetDtoByIdAsync<SubmissionAuthDto>(id, cancellationToken);
		}
	}
}