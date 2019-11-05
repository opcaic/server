using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.MatchExecutions.Queries
{
	public class GetMatchExecutionQuery
		: EntityRequestQuery<MatchExecution>, IRequest<MatchExecutionDetailDto>
	{
		/// <inheritdoc />
		public GetMatchExecutionQuery(long id) : base(id)
		{
		}

		public class Handler : EntityRequestHandler<GetMatchExecutionQuery, MatchExecutionDetailDto>
		{
			private readonly ILogStorageService logStorage;
			private readonly IStorageService storage;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<MatchExecution> repository,
				ILogStorageService logStorage, IStorageService storage) : base(mapper, repository)
			{
				this.logStorage = logStorage;
				this.storage = storage;
			}

			/// <inheritdoc />
			public override async Task<MatchExecutionDetailDto> Handle(
				GetMatchExecutionQuery request, CancellationToken cancellationToken)
			{
				var dto = await base.Handle(request, cancellationToken);

				GatherLogs(dto);
				GatherFiles(dto);

				return dto;
			}

			private void GatherLogs(MatchExecutionDetailDto dto)
			{
				var logs =
					logStorage.GetMatchExecutionLogs(new MatchExecutionStorageDto {Id = dto.Id});

				for (var i = 0; i < dto.BotResults.Count; i++)
				{
					var detail =
						mapper.Map<MatchExecutionDetailDto.SubmissionResultDetailDto>(
							dto.BotResults[i]);

					// the log collection may be "too short"
					if (i < logs.CompilerLogs.Count)
					{
						detail.CompilerLog = logs.CompilerLogs[i];
					}

					dto.BotResults[i] = detail;
				}

				dto.ExecutorLog = logs.ExecutorLog;
			}

			private void GatherFiles(MatchExecutionDetailDto dto)
			{
				using var archive =
					storage.ReadMatchResultArchive(new MatchExecutionStorageDto {Id = dto.Id});

				// not executed yet
				if (archive == null)
					return;

				using var zip = new ZipArchive(archive, ZipArchiveMode.Read, true);

				// filter out logs
				foreach (var e in zip.Entries.Where(e => !Utils.IsMaskedFile(e.Name)))
				{
					dto.AdditionalFiles.Add(
						new MatchExecutionDetailDto.FileDto {Filename = e.Name, Length = e.Length});
				}
			}
		}
	}
}