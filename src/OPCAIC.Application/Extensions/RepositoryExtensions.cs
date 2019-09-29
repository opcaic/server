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
			long id, TDto dto, CancellationToken cancellationToken = default)
			where TEntity : IEntity
		{
			return repository.UpdateAsync(
				new BaseSpecification<TEntity>().AddCriteria(e => e.Id == id), dto, cancellationToken);
		}

		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default)
		{
			var spec = new BaseSpecification<TEntity>().AddCriteria(criteria);
			return repository.ExistsAsync(spec, cancellationToken);
		}

		public static Task<bool> ExistsAsync<TEntity>(this IRepository<TEntity> repository, long id, CancellationToken cancellationToken = default)
			where TEntity : IEntity
		{
			var spec = new BaseSpecification<TEntity>().AddCriteria(e => e.Id == id);
			return repository.ExistsAsync(spec, cancellationToken);
		}

		public static Task<TDto> FindAsync<TEntity, TDto>(this IRepository<TEntity> repository,
			long id, IMapper mapper, CancellationToken cancellationToken = default)
			where TEntity : IEntity
			where TDto : class
		{
			var spec = ProjectingSpecification<TEntity>.Create<TDto>(mapper);
			spec.AddCriteria(e => e.Id == id);

			return repository.FindAsync(spec, cancellationToken);
		}

		public static Task<TDto> FindAsync<TEntity, TDto>(this IRepository<TEntity> repository,
			Expression<Func<TEntity, bool>> criteria, Expression<Func<TEntity, TDto>> projection, CancellationToken cancellationToken = default)
		{
			var spec = ProjectingSpecification<TEntity>.Create(projection);
			spec.AddCriteria(criteria);

			return repository.FindAsync(spec, cancellationToken);
		}

		public static Task<TDto> FindAsync<TEntity, TDto>(this IRepository<TEntity> repository,
			long id, Expression<Func<TEntity, TDto>> projection, CancellationToken cancellationToken = default)
			where TEntity : IEntity
		{
			var spec = ProjectingSpecification<TEntity>.Create(projection);
			spec.AddCriteria(e => e.Id == id);

			return repository.FindAsync(spec, cancellationToken);
		}

		public static Task<TEntity> FindAsync<TEntity>(this IRepository<TEntity> repository,
			long id, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
		{
			return repository.FindAsync(e => e.Id == id, cancellationToken);
		}

		public static Task<TEntity> FindAsync<TEntity>(this IRepository<TEntity> repository,
			Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default)
		{
			return repository.FindAsync(new BaseSpecification<TEntity>()
				.AddCriteria(criteria), cancellationToken);
		}

		public static async Task<TDto> GetAsync<TEntity, TDto>(this IRepository<TEntity> repository,
			long id, IMapper mapper, CancellationToken cancellationToken = default)
			where TEntity : IEntity
			where TDto : class
		{
			return await repository.FindAsync<TEntity, TDto>(id, mapper, cancellationToken) ??
				throw new NotFoundException(typeof(TEntity).Name, id);
		}

		public static async Task<TDto> GetAsync<TEntity, TDto>(this IRepository<TEntity> repository,
			long id, Expression<Func<TEntity, TDto>> projection, CancellationToken cancellationToken = default)
			where TEntity : IEntity
			where TDto : class
		{
			return await repository.FindAsync(id, projection, cancellationToken) ?? 
				throw new NotFoundException(typeof(TEntity).Name, id);
		}

		public static async Task<TDto> GetStructAsync<TEntity, TDto>(this IRepository<TEntity> repository,
			long id, Expression<Func<TEntity, TDto>> projection, CancellationToken cancellationToken = default)
			where TEntity : IEntity
			where TDto : struct
		{
			// query into a list so that we can distinguish not found errors
			var spec = new ProjectingSpecification<TEntity,TDto>(projection);
			spec.WithPaging(0, 1);

			var list = await repository.ListAsync(spec, cancellationToken);
			if (list.Count == 1)
			{
				// found
				return list[0];
			}

			throw new NotFoundException(typeof(TEntity).Name, id);
		}

		public static async Task<TEntity> GetAsync<TEntity>(this IRepository<TEntity> repository,
			long id, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
		{
			return await repository.FindAsync(id, cancellationToken) ??
				throw new NotFoundException(typeof(TEntity).Name, id);
		}

		public static Task<List<TEntity>> ListAsync<TEntity>(this IRepository<TEntity> repository,
			Expression<Func<TEntity, bool>> criteria, CancellationToken cancellationToken = default)
			where TEntity : class
		{
			return repository.ListAsync(
				new BaseSpecification<TEntity>().AddCriteria(criteria), cancellationToken);
		}

		public static Task<List<TDestination>> ListAsync<TEntity, TDestination>(this IRepository<TEntity> repository,
			Expression<Func<TEntity, bool>> criteria, Expression<Func<TEntity, TDestination>> projection, CancellationToken cancellationToken = default)
			where TEntity : class, IEntity
		{
			var spec = new ProjectingSpecification<TEntity,TDestination>(projection);
			spec.AddCriteria(criteria);

			return repository.ListAsync( spec, cancellationToken);
		}
	}
}