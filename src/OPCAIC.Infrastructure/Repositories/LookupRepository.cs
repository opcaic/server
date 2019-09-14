using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Domain.Entities;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;

namespace OPCAIC.Infrastructure.Repositories
{
	public abstract class LookupRepository<TEntity, TFilterDto, TPreviewDto, TDetailDto>
		: RepositoryBase<TEntity>
		where TEntity : class, IEntity
		where TFilterDto : FilterDtoBase
		where TPreviewDto : class
	{
		private readonly Func<DbSet<TEntity>, TFilterDto, IQueryable<TEntity>> applyFilterFunc;

		/// <inheritdoc />
		protected LookupRepository(DataContext context, IMapper mapper,
			Func<DbSet<TEntity>, TFilterDto, IQueryable<TEntity>> applyFilterFunc) : base(context,
			mapper)
		{
			this.applyFilterFunc = applyFilterFunc;
		}

		public async Task<ListDto<TPreviewDto>> GetByFilterAsync(TFilterDto filter,
			CancellationToken cancellationToken)
		{
			var query = applyFilterFunc(DbSet, filter);

			return new ListDto<TPreviewDto>
			{
				List = await query
					.Skip(filter.Offset)
					.Take(filter.Count)
					.ProjectTo<TPreviewDto>(Mapper.ConfigurationProvider)
					.ToListAsync(cancellationToken),
				Total = await query.CountAsync(cancellationToken)
			};
		}

		public Task<TDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<TDetailDto>(id, cancellationToken);
		}
	}
}