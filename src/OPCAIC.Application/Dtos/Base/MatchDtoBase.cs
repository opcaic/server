using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.BaseDtos
{
	public class MatchDtoBase : ICustomMapping
	{
		/// <summary>
		///     Id of the match.
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		///     Id of the tournament the match belongs to.
		/// </summary>
		public long TournamentId { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration
				.CreateMap<Match, MatchDtoBase>(MemberList.Destination)
				.IncludeAllDerived();
		}
	}
}