using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.Serialization
{
	public static class MvcBuilderExtensions
	{
		public static IMvcBuilder AddCustomJsonConverters(this IMvcBuilder mvcBuilder)
		{
			return mvcBuilder.AddJsonOptions(opt =>
			{
				opt.SerializerSettings.Converters.Add(new MenuItemConverter());
			});
		}
	}
}