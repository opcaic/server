using System;
using System.Linq.Expressions;
using OPCAIC.Utils;

namespace OPCAIC.Application.Specifications
{
	public class BaseSpecification<T> : ISpecification<T>
	{
		/// <inheritdoc />
		public Expression<Func<T, bool>> Criteria { get; set; }

		/// <inheritdoc />
		public Expression<Func<T, object>> OrderBy { get; set; }

		/// <inheritdoc />
		public bool OrderByDescending { get; set; }

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
			bool orderDescending = false)
		{
			OrderBy = selector;
			OrderByDescending = orderDescending;
			return this;
		}

		public BaseSpecification<T> WithPaging(int offset, int count)
		{
			PagingInfo = new PagingInfo(offset, count);
			return this;
		}
	}
}