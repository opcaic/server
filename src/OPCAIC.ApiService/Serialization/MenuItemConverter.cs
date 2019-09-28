using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.ValueObjects;

namespace OPCAIC.ApiService.Serialization
{
	public sealed class MenuItemConverter : DiscriminatedJsonConverter
	{
		public MenuItemConverter()
			: base(new DiscriminatorOptions
			{
				BaseType = typeof(MenuItemDto),
				DiscriminatedTypes =
				{
					(((long)MenuItemType.DocumentLink).ToString(),
						typeof(DocumentLinkMenuItemDto)),
					(((long)MenuItemType.ExternalUrl).ToString(),
						typeof(ExternalUrlMenuItemDto))
				},
				DiscriminatorFieldName = nameof(MenuItem.Type),
				SerializeDiscriminator = true
			})
		{
		}
	}
}