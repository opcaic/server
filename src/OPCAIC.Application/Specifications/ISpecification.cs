using System;
using System.Linq.Expressions;

namespace OPCAIC.Application.Specifications
{
	public interface ISpecification<T> 
	{
		/// <summary>
		///     Expression returning true for instances which should be returned.
		/// </summary>
		Expression<Func<T, bool>> Criteria { get; }
		/// <summary>
		///     Expression returning a value according to which the instances should be ordered.
		/// </summary>
		Expression<Func<T, object>> OrderBy { get; }
		/// <summary>
		///     True if the ordering specified by <see cref="OrderBy"/> should be descending.
		/// </summary>
		bool OrderByDescending { get; }
		/// <summary>
		///     If not null, contains information about pagination of the resulting query.
		/// </summary>
		PagingInfo? PagingInfo { get; }
	}
}