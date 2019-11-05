using System.Collections.Generic;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;

namespace OPCAIC.Application.MatchExecutions.Models
{
	public class MatchExecutionDetailDto : MatchExecutionPreviewDto
	{
		public class SubmissionResultDetailDto : SubmissionResultDto, IMapFrom<SubmissionResultDto>
		{
			[IgnoreMap]
			public string CompilerLog { get; set; }
		}

		public class FileDto
		{
			public string Filename { get; set; }
			public long Length { get; set; }
		}

		[IgnoreMap]
		public string ExecutorLog { get; set; }

		// Bot specific logs will be added to base.BotResults[i] via
		// SubmissionMatchResultDetailModel (deriving from *PreviewModel)

		[IgnoreMap]
		public IList<FileDto> AdditionalFiles { get; } = new List<FileDto>();
	}
}