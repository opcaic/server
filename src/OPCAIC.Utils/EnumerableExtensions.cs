﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OPCAIC.Utils
{
	public static class EnumerableExtensions
	{
		public static TSource ArgExtreme<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector, int direction) where TValue : IComparable<TValue>
		{
			Require.NotNull(items, nameof(items));
			Require.NotNull(selector, nameof(selector));

			TSource extremeItem;
			using (var e = items.GetEnumerator())
			{
				if (!e.MoveNext()) throw new InvalidOperationException("Sequence must be nonempty");
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

		public static TSource ArgMin<TSource, TValue>(this IEnumerable<TSource> items, Func<TSource, TValue> selector) where TValue : IComparable<TValue>
		{
			return items.ArgExtreme(selector, 1);
		}

		public static TSource ArgMax<TSource, TValue>(this IEnumerable<TSource> items, Func<TSource, TValue> selector) where TValue : IComparable<TValue>
		{
			return items.ArgExtreme(selector, -1);
		}
	}
}