using AutoMapper;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.MatchExecutions.Models
{
	public class MatchExecutionPreviewDto
		: MatchExecutionDtoBase<MatchExecutionPreviewDto.SubmissionResultDto>, ICustomMapping
	{
		public class SubmissionResultDto : IMapFrom<SubmissionMatchResult>, IAnonymizable
		{
			public SubmissionReferenceDto Submission { get; set; }
			public double Score { get; set; }
			public EntryPointResult CompilerResult { get; set; }
			public JObject AdditionalData { get; set; }

			/// <inheritdoc />
			public void AnonymizeUsersExcept(long? userId)
			{
				if (Submission.Author?.Id != userId)
				{
					Submission.Author = UserReferenceDto.Anonymous;
				}
			}

			public virtual void AddLogs(MatchExecutionLogsDto.SubmissionLog logs)
			{
			}
		}

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			CreateCustomMapping(configuration);
			configuration.CreateMap<MatchExecution, MatchExecutionPreviewDto>(
				MemberList.Destination);
		}
	}
}