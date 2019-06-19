using System.Threading.Tasks;

namespace OPCAIC.Worker.Services
{
	/// <summary>
	///   Provides methods for downloading files from file server.
	/// </summary>
	public interface IDownloadService
	{
		/// <summary>
		///   Downloads a given file and saves it to provided path.
		/// </summary>
		/// <param name="serverPath">Path of the file on the server.</param>
		/// <param name="localPath">Path where the downloaded file should be stored.</param>
		/// <returns></returns>
		Task DownloadAsync(string serverPath, string localPath);

		/// <summary>
		///   Downloads a given file and returns it as raw bytes.
		/// </summary>
		/// <param name="serverPath">Path of the file on the server.</param>
		/// <returns></returns>
		Task<byte[]> DownloadBinaryAsync(string serverPath);

		/// <summary>
		///   Downloads a given file and returns it as text string.
		/// </summary>
		/// <param name="serverPath">Path of the file on the server.</param>
		/// <returns></returns>
		Task<string> DownloadTextAsync(string serverPath);

		/// <summary>
		///   Uploads a file to the given path on the file server.
		/// </summary>
		/// <param name="serverPath">Path where the file should be saved on the server.</param>
		/// <param name="localPath">Path to the file to be uploaded.</param>
		/// <param name="post">If true, POST method is used, otherwise PUT</param>
		/// <returns></returns>
		Task UploadAsync(string serverPath, string localPath, bool post = true);

		/// <summary>
		///   Uploads raw byte buffer to the given path on the file server.
		/// </summary>
		/// <param name="serverPath">Path where the file should be saved on the server.</param>
		/// <param name="data">Raw data to be uploaded.</param>
		/// <param name="post">If true, POST method is used, otherwise PUT</param>
		/// <returns></returns>
		Task UploadBinaryAsync(string serverPath, byte[] data, bool post = true);

		/// <summary>
		///   Uploads a text string as a file to the given path on the file server.
		/// </summary>
		/// <param name="serverPath">Path where the file should be saved on the server.</param>
		/// <param name="data">Raw data to be uploaded.</param>
		/// <param name="post">If true, POST method is used, otherwise PUT</param>
		/// <returns></returns>
		Task UploadTextAsync(string serverPath, string data, bool post = true);
	}
}
