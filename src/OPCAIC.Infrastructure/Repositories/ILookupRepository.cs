using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ILookupRepository<in TFilterDto, TPreviewDto, TDetailDto>
		where TPreviewDto : class
	{
		/// <summary>
		///     Checks whether entity with given id exists.
		/// </summary>
		/// <param name="id">Id of the entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken);

		/// <summary>
		///     Returns list of preview dtos of entities filtered by given filter.
		/// </summary>
		/// <param name="filter">The entity filter.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<ListDto<TPreviewDto>> GetByFilterAsync(TFilterDto filter,
			CancellationToken cancellationToken);

		/// <summary>
		///     Returns detailed dto of the entity with given id.
		/// </summary>
		/// <param name="id">Id of the entity.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<TDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);
	}
}