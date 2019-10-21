using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Specifications;
using OPCAIC.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

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

		public static IQueryable<T> ApplyEntityPreferences<T>(this IQueryable<T> query,
			ISpecification<T> spec) where T : class
		{
			foreach (var expr in spec.Includes)
			{
				query = query.Include(expr);
			}

			if (spec.ReadOnly)
			{
				query = query.AsNoTracking();
			}

			return query;
		}

		public static IQueryable<T> ApplyPaging<T, V>(this IQueryable<T> query, ISpecification<V> spec, bool required = true)
		{
			Require.That<InvalidOperationException>(!required || spec.PagingInfo.HasValue, "Specification does not contain paging info.");

			if (spec.PagingInfo.HasValue)
			{
				var (offset, count) = spec.PagingInfo.Value;
				query = query.Skip(offset).Take(count);
			}

			return query;
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