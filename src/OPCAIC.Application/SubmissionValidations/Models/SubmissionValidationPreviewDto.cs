using System;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.SubmissionValidations.Models
{
	public class SubmissionValidationPreviewDto : ICustomMapping
	{
		public long Id { get; set; }

		public long SubmissionId { get; set; }

		public long TournamentId { get; set; }

		public WorkerJobState State { get; set; }

		public EntryPointResult CheckerResult { get; set; }

		public EntryPointResult CompilerResult { get; set; }

		public EntryPointResult ValidatorResult { get; set; }

		public DateTime? Executed { get; set; }

		/// <inheritdoc />
		public void CreateMapping(Profile configuration)
		{
			configuration
				.CreateMap<SubmissionValidation, SubmissionValidationPreviewDto>(MemberList
					.Destination)
				.ForMember(d => d.TournamentId, opt => opt.MapFrom(d => d.Submission.TournamentId))
				.IncludeAllDerived();
		}
	}
}