using System;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Domain.ValueObjects
{
	public abstract class MenuItem : Entity
	{
		// cannot make this abstract becasue EF cannot map get-only field as discriminator yet
		/// <summary>
		///     Type of the menu item.
		/// </summary>
		public MenuItemType Type { get; protected set; }
	}

	public class ExternalUrlMenuItem : MenuItem
	{
		public ExternalUrlMenuItem()
		{
			Type = MenuItemType.ExternalUrl;
		}

		/// <summary>
		///     Text displayed in the menu item.
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		///     Link to the external resource.
		/// </summary>
		public string ExternalLink { get; set; }
	}

	public class DocumentLinkMenuItem : MenuItem
	{
		public DocumentLinkMenuItem()
		{
			Type = MenuItemType.DocumentLink;
		}

		/// <summary>
		///     Document linked by this menu item.
		/// </summary>
		public virtual Document Document { get; set; }

		/// <summary>
		///     Id of the document linked by this menu item.
		/// </summary>
		public long DocumentId { get; set; }
	}

	public enum MenuItemType
	{
		DocumentLink,
		ExternalUrl
	}
}