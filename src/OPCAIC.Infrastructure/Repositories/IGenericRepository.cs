namespace OPCAIC.Infrastructure.Repositories
{
	public interface IGenericRepository<in TFilterDto, TPreviewDto, TDetailDto, in TNewDto,
			in TUpdateDto>
		: IFilterRepository<TFilterDto, TPreviewDto>,
			ICreateRepository<TNewDto>,
			ILookupRepository<TDetailDto>,
			IUpdateRepository<TUpdateDto>
		where TPreviewDto : class
	{
	}
}