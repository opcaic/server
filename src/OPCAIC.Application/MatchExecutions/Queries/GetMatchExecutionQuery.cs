using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.MatchExecutions.Queries
{
	public class GetMatchExecutionQuery
		: GetMatchExecutionQueryBase, IRequest<MatchExecutionDetailDto>
	{
		/// <inheritdoc />
		public GetMatchExecutionQuery(long id) : base(id)
		{
		}

		public class Handler
			: HandlerBase<GetMatchExecutionQuery, MatchExecutionDetailDto,
				MatchExecutionDetailDto.SubmissionResultDetailDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<MatchExecution> repository,
				ILogStorageService logStorage, IStorageService storage) : base(mapper, repository,
				logStorage, storage)
			{
			}
		}
	}

	public class GetMatchExecutionAdminQuery
		: GetMatchExecutionQueryBase, IRequest<MatchExecutionAdminDto>
	{
		/// <inheritdoc />
		public GetMatchExecutionAdminQuery(long id) : base(id)
		{
		}

		public class Handler
			: HandlerBase<GetMatchExecutionAdminQuery, MatchExecutionAdminDto,
				MatchExecutionAdminDto.SubmissionResultAdminDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<MatchExecution> repository,
				ILogStorageService logStorage, IStorageService storage) : base(mapper, repository,
				logStorage, storage)
			{
			}
		}
	}

	public abstract class GetMatchExecutionQueryBase
		: EntityRequestQuery<MatchExecution>, IAnonymize
	{
		/// <inheritdoc />
		protected GetMatchExecutionQueryBase(long id) : base(id)
		{
		}

		/// <inheritdoc />
		public bool? Anonymize { get; set; }

		public abstract class HandlerBase<TRequest, TResponse, TSubmission>
			: IRequestHandler<TRequest, TResponse>
			where TResponse : MatchExecutionDetailDtoBase<TSubmission>
			where TSubmission : IAnonymizable
			where TRequest : GetMatchExecutionQueryBase, IRequest<TResponse>
		{
			private readonly ILogStorageService logStorage;
			private readonly IMapper mapper;
			private readonly IRepository<MatchExecution> repository;
			private readonly IStorageService storage;

			/// <inheritdoc />
			protected HandlerBase(IMapper mapper, IRepository<MatchExecution> repository,
				ILogStorageService logStorage, IStorageService storage)
			{
				this.mapper = mapper;
				this.repository = repository;
				this.logStorage = logStorage;
				this.storage = storage;
			}

			/// <inheritdoc />
			public async Task<TResponse> Handle(TRequest request,
				CancellationToken cancellationToken)
			{
				var data =
					await repository.GetQueryDataAsync<MatchExecution, TResponse>(
						request.Id, e => e.Match.Tournament, mapper, cancellationToken);

				var dto = data.Dto;

				data.AnonymizeIfNecessary(request);

				var storageDto = new MatchExecutionDtoBase { Id = dto.Id, TournamentId = dto.Match.TournamentId, MatchId = dto.Match.Id };

				var logs =
					logStorage.GetMatchExecutionLogs(storageDto);

				dto.AdditionalFiles.AddRange(FileDto
					.GetFilesInArchive(storage.ReadMatchResultArchive(storageDto))
					.Where(f => !Utils.IsMaskedFile(f.Filename)));

				dto.AddLogs(logs);
				return dto;
			}
		}
	}
}