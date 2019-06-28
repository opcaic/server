using System;
using System.Collections.Generic;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repository;

namespace OPCAIC.Services
{
	class BracketsMatchGenerator : IDeadlineMatchGenerator
	{
		private IMatchRepository matchRepository;

		public BracketsMatchGenerator(IMatchRepository matchRepository)
		{
			this.matchRepository = matchRepository;
		}

		/// <inheritdoc />
		public (IEnumerable<Match> matches, bool done) Generate(Tournament tournament)
		{
			// here we need already executed matches...
			throw new NotImplementedException();
		}
	}
}