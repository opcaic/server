using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using OPCAIC.Application.Extensions;
using OPCAIC.Utils;

namespace OPCAIC.Application.Specifications
{
	/// <summary>
	///     Base class for all projecting specifications.
	/// </summary>
	/// <typeparam name="TSource"></typeparam>
	/// <typeparam name="TDestination"></typeparam>
	public class ProjectingSpecification<TSource, TDestination>
		: BaseSpecification<TSource>, IProjectingSpecification<TSource, TDestination>
	{
		/// <inheritdoc />
		private readonly List<Ordering<TDestination>> orderByProjected;

		/// <inheritdoc />
		public ProjectingSpecification(Expression<Func<TSource, TDestination>> projection)
		{
			Projection = projection;
			orderByProjected = new List<Ordering<TDestination>>();
		}

		/// <inheritdoc />
		public Expression<Func<TSource, TDestination>> Projection { get; }

		/// <inheritdoc />
		public IEnumerable<Ordering<TDestination>> OrderByProjected => orderByProjected;

		public ProjectingSpecification<TSource, TDestination> OrderedProjection(
			Expression<Func<TDestination, object>> selector,
			bool ascending = true)
		{
			orderByProjected.Add(new Ordering<TDestination>(selector, ascending));
			return this;
		}
	}

	/// <summary>
	///     Helper class for creating specification with anonymous types.
	/// </summary>
	public static class ProjectingSpecification<TSource>
	{
		public static ProjectingSpecification<TSource, TDestination> Create<TDestination>(
			Expression<Func<TSource, TDestination>> projection)
		{
			return new ProjectingSpecification<TSource, TDestination>(projection);
		}

		public static ProjectingSpecification<TSource, TDestination> Create<TDestination>(
			IMapper mapper)
		{
			return Create(mapper.GetMapExpression<TSource, TDestination>());
		}
	}

	/// <summary>
	///     Helper class for creating specification with anonymous types.
	/// </summary>
	public static class ProjectingSpecification
	{
		public static ProjectingSpecification<TSource, TDestination> Create<TSource, TDestination>(
			Expression<Func<TSource, TDestination>> projection)
		{
			return ProjectingSpecification<TSource>.Create(projection);
		}
	}
}