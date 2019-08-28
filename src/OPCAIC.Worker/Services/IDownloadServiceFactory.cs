namespace OPCAIC.Worker.Services
{
	public interface IDownloadServiceFactory
	{
		/// <summary>
		///     Creates a new download service which uses specified access token for authorization.
		/// </summary>
		/// <param name="accessToken"></param>
		/// <returns></returns>
		IDownloadService Create(string accessToken);
	}
}