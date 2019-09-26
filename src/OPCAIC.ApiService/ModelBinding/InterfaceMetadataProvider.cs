using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.ModelBinding
{
	public abstract class InterfaceMetadataProvider : IBindingMetadataProvider
	{
		private readonly Type interfaceType;

		protected InterfaceMetadataProvider(Type interfaceType)
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
					ConfigureBinding(context);
				}
			}
		}

		protected abstract void ConfigureBinding(BindingMetadataProviderContext context);
	}
}