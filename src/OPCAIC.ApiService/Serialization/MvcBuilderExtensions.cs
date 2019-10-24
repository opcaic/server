using System;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OPCAIC.ApiService.Utils;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Domain.Enumerations;
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

				foreach (var (type, useName) in EnumerationConfig.GetAnnotatedTypes())
				{
					var converterType = useName
						? typeof(EnumerationNameConverter<>)
						: typeof(EnumerationIdConverter<>);

						opt.SerializerSettings.Converters.Add((JsonConverter)Activator.CreateInstance(converterType.MakeGenericType(type)));
				}

				// configure DateTime handling
				opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
				opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			});
		}
	}
}