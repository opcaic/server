namespace OPCAIC.Application.Interfaces
{
	public interface IWorkerUrlGenerator
	{
		/// <summary>
		///     Generates url used by workers to download additional files for given tournament.
		/// </summary>
		/// <param name="tournamentId">Id of the tournament</param>
		/// <returns></returns>
		string GenerateAdditionalFilesUrl(long tournamentId);
	}
}