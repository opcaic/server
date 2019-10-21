using System;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.ApiService.Serialization
{
	public static class MvcBuilderExtensions
	{
		public static IMvcBuilder ConfigureJsonOptions(this IMvcBuilder mvcBuilder)
		{
			return mvcBuilder.AddNewtonsoftJson(opt =>
			{
				opt.SerializerSettings.Converters.Add(new MenuItemConverter());

				foreach (var enumType in Enumeration.GetAllEnumerationTypes())
				{
					opt.SerializerSettings.Converters.Add((JsonConverter) Activator.CreateInstance(typeof(EnumerationIdConverter<>).MakeGenericType(enumType)));
				}

				// configure DateTime handling
				opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
				opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			});
		}
	}
}