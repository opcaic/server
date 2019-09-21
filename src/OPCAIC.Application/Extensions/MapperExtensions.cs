using System;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using OPCAIC.Utils;

namespace OPCAIC.Application.Extensions
{
	public static class MapperExtensions
	{
		public static Expression<Func<TSource, TDestination>> GetMapExpression<TSource, TDestination>(this IMapper mapper)
		{
			Require.ArgNotNull(mapper, nameof(mapper));
			return mapper.ConfigurationProvider.ExpressionBuilder
				.GetMapExpression<TSource, TDestination>();
		}
	}
}