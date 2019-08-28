using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ISubmissionValidationRepository
	{
		Task<SubmissionValidationStorageDto> FindStorageAsync(long id, CancellationToken cancellationToken);
	}
}