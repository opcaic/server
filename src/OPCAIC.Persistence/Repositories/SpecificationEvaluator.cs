using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using OPCAIC.Application.Specifications;
using OPCAIC.Utils;

namespace OPCAIC.Persistence.Repositories
{
	public static class SpecificationEvaluator
	{
		public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, ISpecification<T> spec)
		{
			if (spec.Criteria != null)
			{
				query = query.Where(spec.Criteria);
			}

			return query;
		}

		public static IQueryable<T> ApplyPaging<T, V>(this IQueryable<T> query, ISpecification<V> spec)
		{
			Require.That<InvalidOperationException>(spec.PagingInfo.HasValue, "Specification does not contain paging info.");
			var (offset, count) = spec.PagingInfo.Value;
			return query.Skip(offset).Take(count);
		}

		public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, IEnumerable<Ordering<T>> ordering)
		{
			using (var it = ordering.GetEnumerator())
			{
				if (!it.MoveNext())
					return query;

				var ordered = it.Current.Ascending
					? query.OrderBy(it.Current.Expression)
					: query.OrderByDescending(it.Current.Expression);

				while (it.MoveNext())
				{
					ordered = it.Current.Ascending
						? ordered.ThenBy(it.Current.Expression)
						: ordered.ThenByDescending(it.Current.Expression);
				}

				query = ordered;
			}

			return query;
		}
	}
}