﻿using System.Collections.Generic;
using System.Linq.Expressions;

namespace OPCAIC.Utils
{
	public class ExpressionParameterRebinder : ExpressionVisitor
	{
		private readonly Dictionary<ParameterExpression, ParameterExpression> map;

		private ExpressionParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
		{
			this.map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
		}

		public static Expression ReplaceParameters(
			Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
		{
			return new ExpressionParameterRebinder(map).Visit(exp);
		}

		protected override Expression VisitParameter(ParameterExpression p)
		{
			if (map.TryGetValue(p, out var replacement))
			{
				p = replacement;
			}

			return base.VisitParameter(p);
		}
	}
}