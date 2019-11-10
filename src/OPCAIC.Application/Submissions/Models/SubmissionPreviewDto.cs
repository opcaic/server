using System;
using AutoMapper;
using OPCAIC.Application.Dtos.Base;
using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Submissions.Models
{
	public class SubmissionPreviewDto : SubmissionDtoBase, ICustomMapping
	{
		public double Score { get; set; }
		public UserReferenceDto Author { get; set; }
		public TournamentReferenceDto Tournament { get; set; }
		public bool IsActive { get; set; }
		public DateTime Created { get; set; }
		public SubmissionValidationState ValidationState { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Submission, SubmissionPreviewDto>(MemberList.Destination)
				.ForMember(s => s.IsActive,
					opt => opt.MapFrom(s => s.TournamentParticipation.ActiveSubmissionId == s.Id))
				.IncludeAllDerived();
		}
	}
}