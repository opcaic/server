using System;
using System.Linq.Expressions;

namespace OPCAIC.Application.Specifications
{
	public struct Ordering<T>
	{
		public Ordering(Expression<Func<T, object>> orderBy, bool ascending)
		{
			Expression = orderBy;
			Ascending = ascending;
		}

		/// <summary>
		///     Expression returning a value according to which the instances should be ordered.
		/// </summary>
		public Expression<Func<T, object>> Expression { get; }
		/// <summary>
		///     True if the ordering should be ascending.
		/// </summary>
		public bool Ascending { get; }
	}
}