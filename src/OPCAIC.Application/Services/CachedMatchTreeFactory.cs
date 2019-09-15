using Microsoft.Extensions.Caching.Memory;
using OPCAIC.Application.Interfaces;

namespace OPCAIC.Application.Services
{
	/// <summary>
	///     Simple cached factory for match trees.
	/// </summary>
	public class CachedMatchTreeFactory
		: IMatchTreeFactory
	{
		private readonly IMemoryCache cache;

		public CachedMatchTreeFactory(IMemoryCache cache)
		{
			this.cache = cache;
		}

		/// <inheritdoc />
		public SingleEliminationTree GetSingleEliminationTree(int participants,
			bool singleThirdPlace)
		{
			return cache.GetOrCreate((participants, singleThirdPlace),
				entry => MatchTreeGenerator.GenerateSingleElimination(participants,
					singleThirdPlace));
		}

		/// <inheritdoc />
		public DoubleEliminationTree GetDoubleEliminationTree(int participants)
		{
			return cache.GetOrCreate(participants,
				entry => MatchTreeGenerator.GenerateDoubleElimination(participants));
		}
	}
}