using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OPCAIC.Application.Specifications
{
	public interface IProjectingSpecification<TSource, TDestination> : ISpecification<TSource>
	{
		/// <summary>
		///     Expression projecting the source type to the target type.
		/// </summary>
		Expression<Func<TSource, TDestination>> Projection { get; }

		/// <summary>
		///     Contains specification for ordering the result.
		/// </summary>
		IEnumerable<Ordering<TDestination>> OrderByProjected { get; }
	}
}