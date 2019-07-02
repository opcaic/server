using System;
using System.Collections.Generic;

namespace OPCAIC.Utils
{
	public static class EnumerableExtensions
	{
		private static TSource ArgExtreme<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector, int direction, bool allowNull)
			where TValue : IComparable<TValue>
		{
			Require.ArgNotNull(items, nameof(items));
			Require.ArgNotNull(selector, nameof(selector));

			TSource extremeItem;
			using (var e = items.GetEnumerator())
			{
				if (!e.MoveNext())
				{
					if (allowNull)
					{
						return default;
					}

					throw new InvalidOperationException("Sequence must be nonempty");
				}

				extremeItem = e.Current;
				var extreme = selector(extremeItem);

				while (e.MoveNext())
				{
					var value = selector(e.Current);
					if (Comparer<TValue>.Default.Compare(value, extreme) * direction < 0)
					{
						extremeItem = e.Current;
						extreme = value;
					}
				}
			}

			return extremeItem;
		}

		public static TSource ArgMin<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector) where TValue : IComparable<TValue>
			=> items.ArgExtreme(selector, 1, false);

		public static TSource ArgMinOrDefault<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector) where TValue : IComparable<TValue>
			=> items.ArgExtreme(selector, 1, true);

		public static TSource ArgMax<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector) where TValue : IComparable<TValue>
			=> items.ArgExtreme(selector, -1, false);

		public static TSource ArgMaxOrDefault<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector) where TValue : IComparable<TValue>
			=> items.ArgExtreme(selector, -1, true);
	}
}
