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
		private readonly FileServerConfig config;

		public DownloadServiceFactory(IOptions<FileServerConfig> config)
		{
			this.config = config.Value;
		}

		public IDownloadService Create(string accessToken)
		{
			var httpClient = new HttpClient {BaseAddress = new Uri(config.ServerAddress)};
			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			return new DownloadService(httpClient);
		}
	}
}