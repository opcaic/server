using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper;

namespace OPCAIC.Application.Infrastructure.AutoMapper
{
	public static class AutoMapperHelpers
	{
		public static IEnumerable<PropertyInfo> GetNotMappedProperties(this Type type)
		{
			return type.GetProperties()
				.Where(t => t.GetCustomAttribute<IgnoreMapAttribute>() != null);
		}

		public static IMappingExpression<TSource, TDestination> IgnoreSourceProperty<TSource, TDestination>(
			this IMappingExpression<TSource, TDestination> map, Expression<Func<TSource, object>> propertySelector)
		{
			return map.ForSourceMember(propertySelector, opt => opt.DoNotValidate());
		}

		public static IMappingExpression<TSource, TDestination> IgnoreProperty<TSource, TDestination>(
			this IMappingExpression<TSource, TDestination> map, Expression<Func<TDestination, object>> propertySelector)
		{
			return map.ForMember(propertySelector, opt => opt.Ignore());
		}
	}
}