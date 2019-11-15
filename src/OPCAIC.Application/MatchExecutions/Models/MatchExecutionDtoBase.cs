using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.MatchExecutions.Models
{
	public abstract class MatchExecutionDtoBase<TSubmissionResult>
		: MatchExecutionDtoBase, IAnonymizable, ICustomMapping 
		where TSubmissionResult : IAnonymizable
	{
		public EntryPointResult ExecutorResult { get; set; }
		public List<TSubmissionResult> BotResults { get; set; }
		public DateTime? Executed { get; set; }
		public DateTime Created { get; set; }
		public JObject AdditionalData { get; set; }
		public MatchReferenceDto Match { get; set; }
		public WorkerJobState State { get; set; }
		public Guid JobId { get; set; }

		/// <inheritdoc />
		protected static void CreateCustomMapping(Profile configuration)
		{
			configuration
				.CreateMap<MatchExecution, MatchExecutionDtoBase<TSubmissionResult>>(MemberList
					.Destination)
				.ForMember(d => d.BotResults,
					opt => opt.MapFrom(e => e.BotResults.OrderBy(b => b.Order)))
				.IncludeAllDerived();
		}

		public void AnonymizeUsersExcept(long? userId)
		{
			foreach (var result in BotResults)
			{
				result.AnonymizeUsersExcept(userId);
			}
		}

		public MatchState ComputeMatchState() => State switch
		{
			WorkerJobState.Waiting => MatchState.Queued,
			WorkerJobState.Scheduled => MatchState.Queued,
			WorkerJobState.Cancelled => MatchState.Cancelled,
			WorkerJobState.Finished =>
			ExecutorResult switch
			{
				EntryPointResult.Success => MatchState.Executed,
				EntryPointResult.Cancelled => MatchState.Cancelled,
				_ => MatchState.Failed
			},
			WorkerJobState.Blocked => MatchState.Cancelled,
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}