﻿using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionMatchResultDto
	{
		public SubmissionReferenceDto Submission { get; set; }
		public double Score { get; set; }
		public string AdditionalData { get; set; }
		public EntryPointResult CompilerResult { get; set; }
	}
}