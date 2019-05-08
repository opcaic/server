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
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			TSource extremeItem;
			TValue extreme;

			using (var e = items.GetEnumerator())
			{
				if (!e.MoveNext()) throw new InvalidOperationException("Sequence must be nonempty");
				extremeItem = e.Current;
				extreme = selector(extremeItem);

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
