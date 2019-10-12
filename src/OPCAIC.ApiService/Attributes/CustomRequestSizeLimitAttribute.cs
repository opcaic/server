using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OPCAIC.ApiService.Configs;

namespace OPCAIC.ApiService.Attributes
{
	public class CustomRequestSizeLimitAttribute : Attribute, IFilterFactory, IOrderedFilter
	{
		public enum Type
		{
			Tournament,
			Submission,
			Result
		}

		private readonly Type type;

		public CustomRequestSizeLimitAttribute(Type type)
		{
			this.type = type;
		}

		/// <inheritdoc />
		public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
		{
			// redirect to the original attribute
			var filter = new RequestSizeLimitAttribute(GetMaxRequestSize(serviceProvider));
			return filter.CreateInstance(serviceProvider);
		}

		/// <inheritdoc />
		public bool IsReusable => true;

		/// <inheritdoc />
		public int Order { get; } = 900; // default for RequestSizeLimitAttribute

		private long GetMaxRequestSize(IServiceProvider serviceProvider)
		{
			var cfg = serviceProvider.GetRequiredService<IOptions<RequestSizeConfig>>().Value;

			switch (type)
			{
				case Type.Tournament:
					return cfg.MaxTournamentFileSize;
				case Type.Submission:
					return cfg.MaxSubmissionFileSize;
				case Type.Result:
					return cfg.MaxResultFileSize;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}