namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IGenericRepository<TDetailDto, in TNewDto,
			in TUpdateDto>
		: ICreateRepository<TNewDto>,
			ILookupRepository<TDetailDto>,
			IUpdateRepository<TUpdateDto>
	{
	}
}