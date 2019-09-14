using Microsoft.Extensions.Caching.Memory;

namespace OPCAIC.Services
{
	/// <summary>
	///     Simple cached factory for match trees.
	/// </summary>
	public class CachedMatchTreeFactory
		: IMatchTreeFactory
	{
		private readonly MemoryCache cache;

		public CachedMatchTreeFactory(MemoryCache cache = null)
		{
			this.cache = cache ?? new MemoryCache(new MemoryCacheOptions());
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