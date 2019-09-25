using System;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Tournaments.Models
{
	public class TournamentPreviewDto : TournamentDtoBase, IMapFrom<Tournament>
	{
		[IgnoreMap]
		public DateTime? LastUserSubmissionDate { get; set; }
	}
}