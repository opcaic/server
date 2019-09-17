using System;
using System.Linq.Expressions;
using OPCAIC.Utils;

namespace OPCAIC.Application.Specifications
{
	public class BaseSpecification<T> : ISpecification<T>
	{
		public BaseSpecification()
		{
		}

		/// <inheritdoc />
		public Expression<Func<T, bool>> Criteria { get; set; }

		public void AddCriteria(Expression<Func<T, bool>> criteria)
		{
			Criteria = Criteria == null 
				? criteria 
				: Criteria.And(criteria);
		}

		/// <inheritdoc />
		public Expression<Func<T, object>> OrderBy { get; set; }

		/// <inheritdoc />
		public bool OrderByDescending { get; set; }

		/// <inheritdoc />
		public PagingInfo? PagingInfo { get; set; }
	}
}