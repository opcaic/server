using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Utils;

namespace OPCAIC.Application.Specifications
{
	public class BaseSpecification<T> : ISpecification<T>
	{
		private readonly List<Ordering<T>> orderBy = new List<Ordering<T>>();

		/// <inheritdoc />
		public bool OrderByDescending { get; set; }

		/// <inheritdoc />
		public Expression<Func<T, bool>> Criteria { get; private set; }

		/// <inheritdoc />
		public IEnumerable<Ordering<T>> OrderBy => orderBy;

		/// <inheritdoc />
		public PagingInfo? PagingInfo { get; set; }

		public BaseSpecification<T> AddCriteria(Expression<Func<T, bool>> criteria)
		{
			Criteria = Criteria == null
				? criteria
				: Criteria.And(criteria);

			return this;
		}

		public BaseSpecification<T> Ordered(Expression<Func<T, object>> selector,
			bool ascending = true)
		{
			orderBy.Add(new Ordering<T>(selector, ascending));
			return this;
		}

		public BaseSpecification<T> WithPaging(int offset, int count)
		{
			PagingInfo = new PagingInfo(offset, count);
			return this;
		}
	}
}