using System.Collections.Generic;
using HybridModelBinding;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.ModelBinding
{
	public class RouteAndBodyBinder : HybridModelBinder
	{
		public RouteAndBodyBinder(IList<IInputFormatter> formatters, IHttpRequestStreamReaderFactory factory) 
			: base(Strategy.FirstInWins)
		{
			AddModelBinder("Body", new BodyModelBinder(formatters, factory));
			AddValueProviderFactory("Route", new RouteValueProviderFactory());
		}
	}

	public class RouteAndBodyBinderProvider : HybridModelBinderProvider
	{
		/// <inheritdoc />
		public RouteAndBodyBinderProvider(IList<IInputFormatter> formatters, IHttpRequestStreamReaderFactory factory) : base(new HybridBindingSource(), new RouteAndBodyBinder(formatters, factory))
		{
		}
	}

	public static class ConfigureHybridBinderExtensions
	{
		public static IMvcBuilder ConfigureHybridBinder(this IMvcBuilder builder)
		{
			var factory = builder.Services.BuildServiceProvider()
				.GetRequiredService<IHttpRequestStreamReaderFactory>();
			return builder.AddMvcOptions(options =>
			{
				options.ModelBinderProviders.Insert(0,
					new RouteAndBodyBinderProvider(options.InputFormatters, factory));
			});
		}
	}
	
}