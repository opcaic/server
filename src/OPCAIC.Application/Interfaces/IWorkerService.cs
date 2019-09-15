using System.Security.Claims;

namespace OPCAIC.Application.Interfaces
{
	public interface IWorkerService
	{
		string GetAdditionalFilesUrl(long tournamentId);

		string GenerateWorkerToken(ClaimsIdentity identity);
	}
}