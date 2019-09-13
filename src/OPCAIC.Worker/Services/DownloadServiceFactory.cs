using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using OPCAIC.Worker.Config;

namespace OPCAIC.Worker.Services
{
	internal class DownloadServiceFactory
		: IDownloadServiceFactory
	{
		private readonly HttpClient httpClient;

		public DownloadServiceFactory(IOptions<FileServerConfig> config)
		{
			httpClient = new HttpClient {BaseAddress = new Uri(config.Value.ServerAddress)};
		}

		public IDownloadService Create(string accessToken)
		{
			var header = new AuthenticationHeaderValue("Bearer", accessToken);
			return new DownloadService(httpClient, header);
		}
	}
}