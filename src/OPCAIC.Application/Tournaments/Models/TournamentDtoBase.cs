using AutoMapper;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using System;
using System.Linq;

namespace OPCAIC.Application.Tournaments.Models
{
	public class TournamentDtoBase : ICustomMapping
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public GameReferenceDto Game { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }

		public DateTime Created { get; set; }

		public TournamentState State { get; set; }

		public int PlayersCount { get; set; }

		public int SubmissionsCount { get; set; }

		public int ActiveSubmissionsCount { get; set; }

		public bool HasAdditionalFiles { get; set; }

		public string ImageUrl { get; set; }

		public double? ImageOverlay { get; set; }

		public string ThemeColor { get; set; }

		public DateTime? Deadline { get; set; }

		public DateTime? Published { get; set; }

		public DateTime? EvaluationStarted { get; set; }

		public DateTime? EvaluationFinished { get; set; }

		public TournamentAvailability Availability { get; set; }

		public int? MatchesPerDay { get; set; }

		public JObject Configuration { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Tournament, TournamentDtoBase>(MemberList
					.Destination)
				.ForMember(d => d.ActiveSubmissionsCount,
					opt => opt.MapFrom(s
						=> s.Participants.Count(e => e.ActiveSubmissionId != null)))
				.ForMember(d => d.PlayersCount,
					opt => opt.MapFrom(s
						=> s.Participants.Count))
				.ForMember(d => d.ImageUrl,
					opt => opt.MapFrom(s
						=> s.ImageUrl ?? s.Game.DefaultTournamentImageUrl))
				.ForMember(d => d.ImageOverlay,
					opt => opt.MapFrom(s
						=> s.ImageOverlay ?? s.Game.DefaultTournamentImageOverlay))
				.ForMember(d => d.ThemeColor,
					opt => opt.MapFrom(s
						=> s.ThemeColor ?? s.Game.DefaultTournamentThemeColor))
				.IncludeAllDerived();
		}
	}
}