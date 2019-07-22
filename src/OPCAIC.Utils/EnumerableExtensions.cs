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

		/// <summary>
		///   Returns the element having the lowest value of the selected property.
		/// </summary>
		/// <typeparam name="TSource">Type of the element.</typeparam>
		/// <typeparam name="TValue">Type of the compared value.</typeparam>
		/// <param name="items">The collection to be enumerated.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>The first element i having the lowest value of selector(i).</returns>
		public static TSource ArgMin<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector) where TValue : IComparable<TValue>
			=> items.ArgExtreme(selector, 1, false);

		/// <summary>
		///   Returns the element having the lowest value of the selected property or default value if
		///   the sequence is empty.
		/// </summary>
		/// <typeparam name="TSource">Type of the element.</typeparam>
		/// <typeparam name="TValue">Type of the compared value.</typeparam>
		/// <param name="items">The collection to be enumerated.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>
		///   The first element i having the highest value of selector(i) or default(TSource) if
		///   the sequence is empty.
		/// </returns>
		public static TSource ArgMinOrDefault<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector) where TValue : IComparable<TValue>
			=> items.ArgExtreme(selector, 1, true);

		/// <summary>
		///   Returns the element having the highest value of the selected property.
		/// </summary>
		/// <typeparam name="TSource">Type of the element.</typeparam>
		/// <typeparam name="TValue">Type of the compared value.</typeparam>
		/// <param name="items">The collection to be enumerated.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>The first element i having the highest value of selector(i).</returns>
		public static TSource ArgMax<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector) where TValue : IComparable<TValue>
			=> items.ArgExtreme(selector, -1, false);

		/// <summary>
		///   Returns the element having the highest value of the selected property or default value if
		///   the sequence is empty.
		/// </summary>
		/// <typeparam name="TSource">Type of the element.</typeparam>
		/// <typeparam name="TValue">Type of the compared value.</typeparam>
		/// <param name="items">The collection to be enumerated.</param>
		/// <param name="selector">The value selector.</param>
		/// <returns>
		///   The first element i having the highest value of selector(i) or default(TSource) if
		///   the sequence is empty.
		/// </returns>
		public static TSource ArgMaxOrDefault<TSource, TValue>(this IEnumerable<TSource> items,
			Func<TSource, TValue> selector) where TValue : IComparable<TValue>
			=> items.ArgExtreme(selector, -1, true);
	}
}
