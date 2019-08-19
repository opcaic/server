using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public abstract class GenericRepository<TEntity, TFilterDto, TPreviewDto, TDetailDto, TNewDto,
			TUpdateDto>
		: LookupRepository<TEntity, TFilterDto, TPreviewDto, TDetailDto>
		where TEntity : class, IEntity
		where TFilterDto : FilterDtoBase
		where TPreviewDto : class
	{
		/// <inheritdoc />
		protected GenericRepository(DataContext context, IMapper mapper,
			Func<DbSet<TEntity>, TFilterDto, IQueryable<TEntity>> applyFilterFunc) : base(context,
			mapper, applyFilterFunc)
		{
		}

		public Task<long> CreateAsync(TNewDto dto, CancellationToken cancellationToken)
		{
			return CreateFromDtoAsync(dto, cancellationToken);
		}

		public Task<bool> UpdateAsync(long id, TUpdateDto dto, CancellationToken cancellationToken)
		{
			return UpdateFromDtoAsync(id, dto, cancellationToken);
		}
	}
}