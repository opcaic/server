using System;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Games.Models
{
	public class GamePreviewDto : ICustomMapping
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public string Key { get; set; }

		public GameType Type { get; set; }

		public DateTime Created { get; set; }

		public int ActiveTournamentsCount { get; set; }

		public string ImageUrl { get; set; }

		/// <inheritdoc />
		public void CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Game, GamePreviewDto>(MemberList.Destination)
				.ForMember(d => d.ActiveTournamentsCount,
					opt => opt.MapFrom(Game.ActiveTournamentCountExpression))
				.IncludeAllDerived();
		}
	}
}