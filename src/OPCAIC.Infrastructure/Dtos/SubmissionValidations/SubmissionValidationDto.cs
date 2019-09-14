using System;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Infrastructure.Dtos.SubmissionValidations
{
	public class SubmissionValidationDto
	{
		public long Id { get; set; }

		public Guid JobId { get; set; }

		public EntryPointResult CheckerResult { get; set; }

		public EntryPointResult CompilerResult { get; set; }

		public EntryPointResult ValidatorResult { get; set; }

		public DateTime? Executed { get; set; }
	}
}