using System;
using AutoMapper;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.SubmissionValidations.Models
{
	public class SubmissionValidationAdminDto : SubmissionValidationDetailDto
	{
		public Guid JobId { get; set; }

		public string Exception { get; set; }

		[IgnoreMap]
		public string CheckerErrorLog { get; set; }

		[IgnoreMap]
		public string CompilerErrorLog { get; set; }

		[IgnoreMap]
		public string ValidatorErrorLog { get; set; }

		public override void AddLogs(SubmissionValidationLogsDto logs)
		{
			base.AddLogs(logs);

			CheckerErrorLog = logs.CheckerErrorLog;
			CompilerErrorLog = logs.CompilerErrorLog;
			ValidatorErrorLog = logs.ValidatorErrorLog;
		}
	}
}