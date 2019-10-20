using System;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Documents.Models
{
	public class DocumentDto : IMapFrom<Document>
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public string Content { get; set; }

		public TournamentReferenceDto Tournament { get; set; }

		public DateTime Created { get; set; }
	}
}