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

		public static IReturnsResult<IRepository<TEntity>> SetupExists<TEntity>(this Mock<IRepository<TEntity>> mock, bool value, CancellationToken cancellationToken)
		{
			return mock.Setup(s => s.ExistsAsync(It.IsAny<ISpecification<TEntity>>(), cancellationToken))
				.ReturnsAsync(value);
		}

		public static IReturnsResult<IRepository<TEntity>> SetupProject<TEntity, TDto>(this Mock<IRepository<TEntity>> mock, TDto value, CancellationToken cancellationToken)
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

		public static IReturnsResult<IRepository<TEntity>> SetupDelete<TEntity>(
			this Mock<IRepository<TEntity>> mock, CancellationToken cancellationToken,
			bool success = true)
		{
			return mock
				.Setup(s => s.DeleteAsync(It.IsAny<ISpecification<TEntity>>(),
					cancellationToken)).ReturnsAsync(success);
		}

		public static ISetup<IRepository<TEntity>> SetupDelete<TEntity>(
			this Mock<IRepository<TEntity>> mock, TEntity entity)
		{
			return mock .Setup(s => s.Delete(entity));
		}

		public static ISetup<IRepository<TEntity>> SetupDelete<TEntity>(
			this Mock<IRepository<TEntity>> mock, Expression<Func<TEntity, bool>> match, CancellationToken cancellationToken,
			bool success = true)
		{
			return mock
				.Setup(s => s.Delete(It.Is(match)));
		}
	}
}