using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.BaseDtos
{
	public class SubmissionValidationDtoBase : ICustomMapping
	{
		/// <summary>
		///     Id of the submission validation.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		///     Id of the submission being validated.
		/// </summary>
		public long SubmissionId { get; set; }

		/// <summary>
		///     Id of the tournament where the submission was originally submitted to.
		/// </summary>
		public long TournamentId { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration
				.CreateMap<SubmissionValidation, SubmissionValidationDtoBase>(
					MemberList.Destination)
				.ForMember(d => d.TournamentId, opt => opt.MapFrom(d => d.Submission.TournamentId))
				.IncludeAllDerived();
		}
	}
}