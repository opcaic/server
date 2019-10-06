using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OPCAIC.ApiService.ModelBinding
{
	public class FromRouteAndBodyAttribute : Attribute, IBindingSourceMetadata
	{
		public static readonly BindingSource RouteAndBodyBindingSource = new BindingSource("RouteAndBody", "RouteAndBody", true, true);

		/// <inheritdoc />
		public BindingSource BindingSource => RouteAndBodyBindingSource;
	}

	public class RouteAndBodyBinder : IModelBinder
	{
		private readonly BodyModelBinder bodyModelBinder;
		private readonly RouteValueProviderFactory routeValueProviderFactory;

		public RouteAndBodyBinder(IOptions<MvcOptions> options, IHttpRequestStreamReaderFactory factory)
		{
			bodyModelBinder = new BodyModelBinder(options.Value.InputFormatters.ToList(), factory);
			routeValueProviderFactory = new RouteValueProviderFactory();
		}

		/// <inheritdoc />
		public async Task BindModelAsync(ModelBindingContext bindingContext)
		{
			await bodyModelBinder.BindModelAsync(bindingContext);
			var hydratedModel = bindingContext.Result.Model;

			if (!bindingContext.ModelState.IsValid)
			{
				// no need to continue if body is invalid
				return;
			}

			var modelProperties = hydratedModel.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

			var routeValueProvider = await GetRouteValueProvider(bindingContext);

			foreach (var propertyInfo in modelProperties)
			{
				var value = routeValueProvider.GetValue(propertyInfo.Name).SingleOrDefault(x => !string.IsNullOrEmpty(x));
				if (value != null)
				{
					SetProperty(value, propertyInfo, hydratedModel);
				}
			}
		}

		private static void SetProperty(string value, PropertyInfo propertyInfo, object hydratedModel)
		{
			var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
			if (converter.CanConvertFrom(value.GetType()))
			{
				propertyInfo.SetValue(hydratedModel, converter.ConvertFrom(value), null);
			}
		}

		private async Task<IValueProvider> GetRouteValueProvider(ModelBindingContext bindingContext)
		{
			IValueProvider routeValueProvider;

			var valueProviderFactoryContext = new ValueProviderFactoryContext(bindingContext.ActionContext);
			{
				await routeValueProviderFactory.CreateValueProviderAsync(valueProviderFactoryContext);
				routeValueProvider = valueProviderFactoryContext.ValueProviders.LastOrDefault();
			}
			return routeValueProvider;
		}
	}

	public class RouteAndBodyBinderProvider : IModelBinderProvider
	{
		/// <inheritdoc />
		public IModelBinder GetBinder(ModelBinderProviderContext context)
		{
			if (context.BindingInfo.BindingSource ==
				FromRouteAndBodyAttribute.RouteAndBodyBindingSource)
				return context.Services.GetRequiredService<RouteAndBodyBinder>();

			return null;
		}
	}

	public static class BindingConfigurationExtensions
	{
		public static IMvcBuilder ConfigureCustomBinders(this IMvcBuilder builder)
		{
			builder.Services.AddSingleton<RouteAndBodyBinder>();

			return builder.AddMvcOptions(options =>
			{
				options.ModelBinderProviders.Insert(0,
					new RouteAndBodyBinderProvider());
			});
		}
	}

}