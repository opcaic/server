using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IUserRepository
		: ILookupRepository<UserDetailDto>,
			IUpdateRepository<UserProfileDto>,
			IRepository<User>

	{
	}
}