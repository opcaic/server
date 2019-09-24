using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Utils;

namespace OPCAIC.ApiService
{
	public class ExcludeInterfaceMetadataProvider : IBindingMetadataProvider
	{
		private readonly Type interfaceType;

		public ExcludeInterfaceMetadataProvider(Type interfaceType)
		{
			Require.ArgNotNull(interfaceType, nameof(interfaceType));
			Require.That<ArgumentException>(interfaceType.IsInterface, "Type is not interface.");

			this.interfaceType = interfaceType;
		}

		/// <inheritdoc />
		public void CreateBindingMetadata(BindingMetadataProviderContext context)
		{
			if (context.Key.ContainerType == null)
			{
				return; // only for properties
			}

			if (interfaceType.IsAssignableFrom(context.Key.ContainerType))
			{
				if (interfaceType.GetProperty(context.Key.Name) != null)
				{
					context.BindingMetadata.IsBindingAllowed = false;
				}
			}

		}
	}
}