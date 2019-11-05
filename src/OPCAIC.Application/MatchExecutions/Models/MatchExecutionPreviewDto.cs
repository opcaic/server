using System;
using System.Collections.Generic;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.MatchExecutions.Models
{
	public class MatchExecutionPreviewDto : MatchDetailDto.ExecutionDto
	{
		public MatchReferenceDto Match { get; set; }
		public WorkerJobState State { get; set; }
		public Guid JobId { get; set; }
	}
}