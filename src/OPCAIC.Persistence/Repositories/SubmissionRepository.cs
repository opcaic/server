﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Submissions.Models;
using OPCAIC.Application.Submissions.Queries;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public class SubmissionRepository
		: GenericRepository<Submission, GetSubmissionsQuery, SubmissionPreviewDto,
				SubmissionDetailDto, NewSubmissionDto, UpdateSubmissionScoreDto>,
			ISubmissionRepository
	{
		/// <inheritdoc />
		public SubmissionRepository(DataContext context, IMapper mapper)
			: base(context, mapper, null)
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
		public Task<SubmissionAuthDto> GetAuthorizationData(long id,
			CancellationToken cancellationToken = default)
		{
			return GetDtoByIdAsync<SubmissionAuthDto>(id, cancellationToken);
		}
	}
}