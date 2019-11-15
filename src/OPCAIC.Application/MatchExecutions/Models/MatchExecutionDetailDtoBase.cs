using System.Collections.Generic;
using AutoMapper;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.MatchExecutions.Models
{
	public abstract class MatchExecutionDetailDtoBase<TSubmissionResult>
		: MatchExecutionDtoBase<TSubmissionResult>, IMapFrom<MatchExecution>
		where TSubmissionResult : IAnonymizable
	{
		[IgnoreMap]
		public List<FileDto> AdditionalFiles { get; } = new List<FileDto>();

		public abstract void AddLogs(MatchExecutionLogsDto logs);
	}
}