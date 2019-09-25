using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OPCAIC.Utils
{
	public sealed class Rebind : ExpressionVisitor
	{
		static readonly Rebind Instance = new Rebind();
		private Rebind()
		{
		}

		public static TDestination Invoke<TSource, TDestination>(
			TSource source, Expression<Func<TSource, TDestination>> projection)
		{
			throw new InvalidOperationException("This method is only a marker in an expression tree and should never be called.");
		}

		public static Expression<Func<TSource, TDestination>> Map<TSource, TDestination>(
			Expression<Func<TSource, TDestination>> expression)
		{
			return Instance.VisitAndConvert(expression, null);
		}

		/// <inheritdoc />
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (!(node.Arguments[1] is MemberExpression projectionArg &&
				projectionArg.Expression is ConstantExpression valueExpression))
			{
				throw new NotSupportedException(
					"Only captured variable outside the lambda expression is supported");
			}

			// get the mapping expression
			var field = (FieldInfo) projectionArg.Member;
			var map = (LambdaExpression) field.GetValue(valueExpression.Value);

			// replace the method call by the lambda's body
			var newValueExpression = ExpressionParameterRebinder.ReplaceParameters(
				new Dictionary<ParameterExpression, Expression>
				{
					[map.Parameters[0]] = node.Arguments[0]
				}, map.Body);
			return newValueExpression;
		}
	}
}