using System.Linq;
using OPCAIC.Application.Specifications;

namespace OPCAIC.Persistence.Repositories
{
	public static class SpecificationEvaluator<T>
	{
		public static IQueryable<T> ApplySpecification(IQueryable<T> query, ISpecification<T> spec)
		{
			if (spec.Criteria != null)
			{
				query = query.Where(spec.Criteria);
			}

			if (spec.OrderBy != null)
			{
				query = spec.OrderByDescending
					? query.OrderByDescending(spec.OrderBy)
					: query.OrderBy(spec.OrderBy);
			}

			if (spec.PagingInfo.HasValue)
			{
				var (offset, count) = spec.PagingInfo.Value;
				query = query.Skip(offset).Take(count);
			}

			return query;
		}

		public static IQueryable<TDestination> ApplyProjection<TDestination>(
			IQueryable<T> query,
			IProjectingSpecification<T, TDestination> spec)
		{
			query = ApplySpecification(query, spec);
			return query.Select(spec.Projection);
		}
	}
}