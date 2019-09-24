using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OPCAIC.Application.Infrastructure;

namespace OPCAIC.Application.Specifications
{
	public interface ISpecification<T> 
	{
		/// <summary>
		///     Expression returning true for instances which should be returned.
		/// </summary>
		Expression<Func<T, bool>> Criteria { get; }
		/// <summary>
		///     Specifies the order in which the result should be ordered.
		/// </summary>
		IEnumerable<Ordering<T>> OrderBy { get; }
		/// <summary>
		///     If not null, contains information about pagination of the resulting query.
		/// </summary>
		PagingInfo? PagingInfo { get; }
	}
}