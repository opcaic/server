﻿using System;
using OPCAIC.Infrastructure.Dtos.Tournaments;

namespace OPCAIC.Infrastructure.Dtos.Documents
{
	public class DocumentDetailDto
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public string Content { get; set; }

		public TournamentReferenceDto Tournament { get; set; }

		public DateTime Created { get; set; }
	}
}