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
			if (node.Method.DeclaringType != typeof(Rebind))
			{
				return base.VisitMethodCall(node);
			}

			if (node.Method.Name != nameof(Invoke))
			{
				throw new InvalidOperationException($"Cannot call {node.Method} inside expression to be rebound.");
			}

			// get the mapping expression
			var map = (LambdaExpression) new EvaluatingVisitor().Evaluate(node.Arguments[1]);

			// replace the method call by the lambda's body
			var newValueExpression = ExpressionParameterRebinder.ReplaceParameters(
				new Dictionary<ParameterExpression, Expression>
				{
					[map.Parameters[0]] = Visit(node.Arguments[0])
				}, map.Body);

			return newValueExpression;
		}

		private class EvaluatingVisitor : ExpressionVisitor
		{
			private object value;

			public object Evaluate(Expression e)
			{
				if (e == null)
				{
					return null;
				}
				Visit(e);
				return value;
			}

			/// <inheritdoc />
			protected override Expression VisitConstant(ConstantExpression node)
			{
				value = node.Value;
				return node;
			}

			/// <inheritdoc />
			protected override Expression VisitMember(MemberExpression node)
			{
				var instance = Evaluate(node.Expression);
				switch (node.Member)
				{
					case FieldInfo fieldInfo:
						value = fieldInfo.GetValue(instance);
						break;
					case PropertyInfo propertyInfo:
						value = propertyInfo.GetValue(instance);
						break;
					default:
						throw new NotSupportedException();
				}

				return node;
			}
		}
	}
}