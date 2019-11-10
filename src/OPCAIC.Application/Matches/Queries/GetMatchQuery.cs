﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Utils;

namespace OPCAIC.Application.Matches.Queries
{
	public class GetMatchQuery
		: GetMatchQueryBase, IRequest<MatchDetailDto>
	{
		/// <inheritdoc />
		public GetMatchQuery(long id) : base(id)
		{
		}

		public class Handler
			: HandlerBase<GetMatchQuery, MatchDetailDto, MatchExecutionDetailDto,
				MatchExecutionDetailDto.SubmissionResultDetailDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IMatchRepository repository, IStorageService storage,
				ILogStorageService logStorage) : base(mapper, repository, storage, logStorage)
			{
			}
		}
	}

	public class GetMatchAdminQuery
		: GetMatchQueryBase, IRequest<MatchAdminDto>
	{
		/// <inheritdoc />
		public GetMatchAdminQuery(long id) : base(id)
		{
		}

		public class Handler
			: HandlerBase<GetMatchAdminQuery, MatchAdminDto, MatchExecutionAdminDto,
				MatchExecutionAdminDto.SubmissionResultAdminDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IMatchRepository repository, IStorageService storage,
				ILogStorageService logStorage) : base(mapper, repository, storage, logStorage)
			{
			}
		}
	}


	public abstract class GetMatchQueryBase
		: AuthenticatedRequest, IAnonymize
	{
		/// <inheritdoc />
		protected GetMatchQueryBase(long id)
		{
			Id = id;
		}

		public long Id { get; }

		public bool? Anonymize { get; set; }

		public abstract class HandlerBase<TRequest, TResponse, TExecution, TSubmission>
			: IRequestHandler<TRequest, TResponse>
			where TResponse : MatchDetailDtoBase<TExecution, TSubmission>
			where TSubmission : IAnonymizable
			where TExecution : MatchExecutionDetailDtoBase<TSubmission>
			where TRequest : GetMatchQueryBase, IRequest<TResponse>
		{
			private readonly ILogStorageService logStorage;
			private readonly IMapper mapper;
			private readonly IMatchRepository repository;
			private readonly IStorageService storage;

			public HandlerBase(IMapper mapper, IMatchRepository repository, IStorageService storage,
				ILogStorageService logStorage)
			{
				this.mapper = mapper;
				this.repository = repository;
				this.storage = storage;
				this.logStorage = logStorage;
			}

			/// <inheritdoc />
			public async Task<TResponse> Handle(TRequest request,
				CancellationToken cancellationToken)
			{
				var map = mapper.GetMapExpression<Match, TResponse>();
				var orgMap = mapper.GetMapExpression<Tournament, TournamentOrganizersDto>();
				var spec = ProjectingSpecification.Create(Rebind.Map((Match m) =>
					new QueryData<TResponse>
					{
						Dto = Rebind.Invoke(m, map),
						OrganizersDto = Rebind.Invoke(m.Tournament, orgMap),
						TournamentAnonymize = m.Tournament.Anonymize
					}
				));

				spec.AddCriteria(m => m.Id == request.Id);

				var data = await repository.GetAsync(spec, cancellationToken);
				data.AnonymizeIfNecessary(request);

				foreach (var execution in data.Dto.Executions)
				{
					execution.AdditionalFiles.AddRange(FileDto
						.GetFilesInArchive(
							storage.ReadMatchResultArchive(
								new MatchExecutionStorageDto {Id = execution.Id}))
						.Where(f => !MatchExecutions.Utils.IsMaskedFile(f.Filename)));

					execution.AddLogs(
						logStorage.GetMatchExecutionLogs(
							new MatchExecutionStorageDto {Id = execution.Id}));
				}

				return data.Dto;
			}
		}
	}
}