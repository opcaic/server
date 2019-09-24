namespace OPCAIC.Application.Infrastructure
{
	/// <summary>
	///     Information about query pagination.
	/// </summary>
	public struct PagingInfo
	{
		public PagingInfo(int offset, int count)
		{
			Offset = offset;
			Count = count;
		}

		public void Deconstruct(out int offset, out int count)
		{
			offset = Offset;
			count = Count;
		}

		/// <summary>
		///     Index of the first element in the result that should be returned.
		/// </summary>
		public int Offset { get; }

		/// <summary>
		///     Number of items that should be returned in the query.
		/// </summary>
		public int Count { get; }
	}
}