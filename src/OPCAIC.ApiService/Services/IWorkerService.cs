using System.Security.Claims;

namespace OPCAIC.ApiService.Services
{
	public interface IWorkerService
	{
		string GetAdditionalFilesUrl(long tournamentId);

		string GenerateWorkerToken(ClaimsIdentity identity);
	}
}