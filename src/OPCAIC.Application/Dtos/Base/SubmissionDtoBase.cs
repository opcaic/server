using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Base
{
	public class SubmissionDtoBase : ICustomMapping
	{
		/// <summary>
		///     Submission id.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		///     Id of the tournament where the submission was originally submitted to.
		/// </summary>
		public long TournamentId { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration
				.CreateMap<Submission, SubmissionDtoBase>(MemberList.Destination)
				.IncludeAllDerived();
		}
	}
}