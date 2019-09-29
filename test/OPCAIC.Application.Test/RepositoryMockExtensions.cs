using System;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using Moq.Language.Flow;
using OPCAIC.Application.Specifications;

namespace OPCAIC.Application.Test
{
	public static class RepositoryMockExtensions
	{
		public static IReturnsResult<IRepository<TEntity>> SetupFind<TEntity>(this Mock<IRepository<TEntity>> mock, TEntity value, CancellationToken cancellationToken)
		{
			return mock.Setup(s => s.FindAsync(It.IsAny<ISpecification<TEntity>>(), cancellationToken))
				.ReturnsAsync(value);
		}

		public static IReturnsResult<IRepository<TEntity>> SetupFind<TEntity, TDto>(this Mock<IRepository<TEntity>> mock, TDto value, CancellationToken cancellationToken)
		{
			return mock.Setup(s => s.FindAsync(It.IsAny<IProjectingSpecification<TEntity, TDto>>(), cancellationToken))
				.ReturnsAsync(value);
		}

		public static IReturnsResult<IRepository<TEntity>> SetupUpdate<TEntity, TDto>(
			this Mock<IRepository<TEntity>> mock, Expression<Func<TDto, bool>> match, CancellationToken cancellationToken,
			bool success = true)
		{
			return mock
				.Setup(s => s.UpdateAsync(It.IsAny<ISpecification<TEntity>>(), It.Is(match),
					cancellationToken)).ReturnsAsync(success);
		}
	}
}