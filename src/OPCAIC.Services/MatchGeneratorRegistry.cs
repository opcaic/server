using System;
using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Services
{
	public class MatchGeneratorRegistry : IMatchGenerator
	{
		private readonly Dictionary<TournamentFormat, IBracketsMatchGenerator> bracketGenerators;
		private readonly Dictionary<TournamentFormat, IDeadlineMatchGenerator> deadlineGenerators;
		private readonly Dictionary<TournamentFormat, IOngoingMatchGenerator> ongoingGenerators;

		public MatchGeneratorRegistry(IEnumerable<IBracketsMatchGenerator> bracketsGen, IEnumerable<IDeadlineMatchGenerator> deadlineGen, IEnumerable<IOngoingMatchGenerator> ongoingGen)
		{
			bracketGenerators = bracketsGen.ToDictionary(g => g.Format);
			deadlineGenerators = deadlineGen.ToDictionary(g => g.Format);
			ongoingGenerators = ongoingGen.ToDictionary(g => g.Format);
		}

		/// <inheritdoc />
		public (List<NewMatchDto> matches, bool done) GenerateBrackets(TournamentBracketsGenerationDto tournament)
		{
			return GetGenerator(bracketGenerators, tournament.Format).Generate(tournament);
		}

		public List<NewMatchDto> GenerateDeadline(TournamentDeadlineGenerationDto tournament)
		{
			return GetGenerator(deadlineGenerators, tournament.Format).Generate(tournament);
		}

		/// <inheritdoc />
		public List<NewMatchDto> GenerateOngoing(TournamentOngoingGenerationDto tournament, int count)
		{
			return GetGenerator(ongoingGenerators, tournament.Format).Generate(tournament, count);
		}

		private TGenerator GetGenerator<TGenerator>(
			Dictionary<TournamentFormat, TGenerator> generators, TournamentFormat format)
			where TGenerator : class
		{
			return generators.GetValueOrDefault(format) ?? 
				throw new InvalidOperationException("Invalid tournament format");
		}
	}
}