using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IFilterRepository<in TFilterDto, TPreviewDto>
		where TPreviewDto : class
	{
		/// <summary>
		///     Returns list of preview dtos of entities filtered by given filter.
		/// </summary>
		/// <param name="filter">The entity filter.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<ListDto<TPreviewDto>> GetByFilterAsync(TFilterDto filter,
			CancellationToken cancellationToken);
	}
}