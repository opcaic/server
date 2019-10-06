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
		private readonly List<string> includes = new List<string>();

		/// <inheritdoc />
		public Expression<Func<T, bool>> Criteria { get; private set; }

		/// <inheritdoc />
		public IEnumerable<Ordering<T>> OrderBy => orderBy;

		/// <inheritdoc />
		public PagingInfo? PagingInfo { get; set; }

		/// <inheritdoc />
		public IEnumerable<string> Includes => includes;

		/// <inheritdoc />
		public bool ReadOnly { get; set; }

		public BaseSpecification<T> AsReadOnly(bool readOnly = true)
		{
			ReadOnly = readOnly;
			return this;
		}

		public BaseSpecification<T> Include(string path)
		{
			includes.Add(path);
			return this;
		}

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