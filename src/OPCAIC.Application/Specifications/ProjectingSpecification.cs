using System;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;

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
		public ProjectingSpecification(Expression<Func<TSource, TDestination>> projection)
		{
			Projection = projection;
		}

		/// <inheritdoc />
		public Expression<Func<TSource, TDestination>> Projection { get; }
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
			Expression<Func<TSource, bool>> criteria,
			Expression<Func<TSource, TDestination>> projection)
		{
			var spec = Create(projection);
			spec.Criteria = criteria;
			return spec;
		}

		public static ProjectingSpecification<TSource, TDestination> Create<TDestination>(
			Expression<Func<TSource, bool>> criteria,
			IMapper mapper)
		{
			var projection = mapper.ConfigurationProvider.ExpressionBuilder
				.GetMapExpression<TSource, TDestination>();
			return Create(criteria, projection);
		}

		public static ProjectingSpecification<TSource, TDestination> Create<TDestination>(
			IMapper mapper)
		{
			var projection = mapper.ConfigurationProvider.ExpressionBuilder
				.GetMapExpression<TSource, TDestination>();
			return Create(projection);
		}
	}
}