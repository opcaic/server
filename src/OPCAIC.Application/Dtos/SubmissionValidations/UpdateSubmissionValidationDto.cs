using System;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.SubmissionValidations
{
	public class UpdateSubmissionValidationDto
	{
		public EntryPointResult CheckerResult { get; set; }

		public EntryPointResult CompilerResult { get; set; }

		public EntryPointResult ValidatorResult { get; set; }

		public WorkerJobState State { get; set; }

		public DateTime? Executed { get; set; }
	}
}