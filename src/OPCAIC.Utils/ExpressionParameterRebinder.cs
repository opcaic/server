using System.Collections.Generic;
using System.Linq.Expressions;

namespace OPCAIC.Utils
{
	public class ExpressionParameterRebinder
	{
		public static Expression ReplaceParameters<TTarget>(
			Dictionary<ParameterExpression, TTarget> map, Expression exp)
			where TTarget : Expression
		{
			return new Visitor<TTarget>(map).Visit(exp);
		}

		private class Visitor<TTarget> : ExpressionVisitor
			where TTarget : Expression
		{
			private readonly Dictionary<ParameterExpression, TTarget> map;

			public Visitor(Dictionary<ParameterExpression, TTarget> map)
			{
				this.map = map;
			}

			protected override Expression VisitParameter(ParameterExpression p)
			{
				if (map.TryGetValue(p, out var replacement))
				{
					return replacement;
				}

				return base.VisitParameter(p);
			}
		}
	}
}