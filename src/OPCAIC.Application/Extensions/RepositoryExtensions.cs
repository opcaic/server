using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Application.Exceptions;

namespace OPCAIC.Application.Extensions
{
	public static class RepositoryExtensions
	{
		public static Task<bool> UpdateAsync<TEntity, TDto>(this IRepository<TEntity> repository,
			long id, TDto dto, CancellationToken cancellationToken)
			where TEntity : IEntity
		{
			return repository.UpdateAsync(
				new BaseSpecification<TEntity>().AddCriteria(e => e.Id == id), dto, cancellationToken);
		}

		public static async Task<TDto> GetAsync<TEntity, TDto>(this IRepository<TEntity> repository,
			long id, IMapper mapper, CancellationToken cancellationToken)
			where TEntity : IEntity
			where TDto : class
		{
			var spec = ProjectingSpecification<TEntity>.Create<TDto>(mapper);
			spec.AddCriteria(e => e.Id == id);

			return await repository.FindAsync(spec, cancellationToken) ?? 
				throw new NotFoundException(typeof(TEntity).Name, id);
		}

		public static async Task<TDto> GetAsync<TEntity, TDto>(this IRepository<TEntity> repository,
			long id, Expression<Func<TEntity, TDto>> projection, CancellationToken cancellationToken)
			where TEntity : IEntity
			where TDto : class
		{
			var spec = ProjectingSpecification<TEntity>.Create(projection);
			spec.AddCriteria(e => e.Id == id);

			return await repository.FindAsync(spec, cancellationToken) ?? 
				throw new NotFoundException(typeof(TEntity).Name, id);
		}

		public static async Task<TEntity> GetAsync<TEntity>(this IRepository<TEntity> repository,
			long id, CancellationToken cancellationToken)
			where TEntity : class, IEntity
		{
			return await repository.FindAsync(
				new BaseSpecification<TEntity>().AddCriteria(e => e.Id == id), cancellationToken) ??
				throw new NotFoundException(typeof(TEntity).Name, id);
		}

		public static Task<List<TEntity>> ListAsync<TEntity>(this IRepository<TEntity> repository,
			Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken)
			where TEntity : class, IEntity
		{
			return repository.ListAsync(
				new BaseSpecification<TEntity>().AddCriteria(criteria), cancellationToken);
		}

		public static Task<List<TDestination>> ListAsync<TEntity, TDestination>(this IRepository<TEntity> repository,
			Expression<Func<TEntity, bool>> criteria, Expression<Func<TEntity, TDestination>> projection, CancellationToken cancellationToken)
			where TEntity : class, IEntity
		{
			var spec = new ProjectingSpecification<TEntity,TDestination>(projection);
			spec.AddCriteria(criteria);

			return repository.ListAsync( spec, cancellationToken);
		}
	}
}