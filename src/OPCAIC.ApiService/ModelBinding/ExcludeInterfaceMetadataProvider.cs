using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace OPCAIC.ApiService.ModelBinding
{
	public class ExcludeInterfaceMetadataProvider : InterfaceMetadataProvider
	{
		public ExcludeInterfaceMetadataProvider(Type interfaceType)
			:base(interfaceType)
		{
		}

		/// <inheritdoc />
		protected override void ConfigureBinding(BindingMetadataProviderContext context)
		{
			context.BindingMetadata.IsBindingAllowed = false;
		}
	}
}