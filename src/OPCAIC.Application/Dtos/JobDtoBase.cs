﻿using System;

namespace OPCAIC.Application.Dtos
{
	public class JobDtoBase
	{
		public Guid JobId { get; set; }
		public long TournamentId { get; set; }
		public string GameKey { get; set; }
		public string TournamentConfiguration { get; set; }
		public DateTime Created { get; set; }
	}
}