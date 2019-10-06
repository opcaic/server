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

		/// <summary>
		///     Set of navigation paths to related properties which should be fetched as well.
		/// </summary>
		IEnumerable<string> Includes { get; }

		/// <summary>
		///     If true, the query is intended to be read-only and no change tracking of returned
		///     entities is to be performed.
		/// </summary>
		bool ReadOnly { get; }
	}
}