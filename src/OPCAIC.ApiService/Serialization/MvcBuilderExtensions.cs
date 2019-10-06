using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace OPCAIC.ApiService.Serialization
{
	public static class MvcBuilderExtensions
	{
		public static IMvcBuilder ConfigureJsonOptions(this IMvcBuilder mvcBuilder)
		{
			return mvcBuilder.AddJsonOptions(opt =>
			{
				opt.SerializerSettings.Converters.Add(new MenuItemConverter());

				// configure DateTime handling
				opt.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
				opt.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
			});
		}
	}
}