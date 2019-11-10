using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.BaseDtos
{
	public class MatchExecutionDtoBase : ICustomMapping
	{
		/// <summary>
		///     Match execution id.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		///     Id of the match this execution is for.
		/// </summary>
		public long MatchId { get; set; }

		/// <summary>
		///     Id of the tournament the match belongs to.
		/// </summary>
		public long TournamentId { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration
				.CreateMap<MatchExecution, MatchExecutionDtoBase>(
					MemberList.Destination)
				.ForMember(d => d.TournamentId, opt => opt.MapFrom(d => d.Match.TournamentId))
				.IncludeAllDerived();
		}
	}
}