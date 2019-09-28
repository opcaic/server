using System;

namespace OPCAIC.Domain.ValueObjects
{
	public abstract class MenuItem
	{
		/// <summary>
		///     Type of the menu item.
		/// </summary>
		public abstract MenuItemType Type { get; }
	}

	public class ExternalUrlMenuItem : MenuItem
	{
		/// <inheritdoc />
		public override MenuItemType Type => MenuItemType.ExternalUrl;

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
		/// <inheritdoc />
		public override MenuItemType Type => MenuItemType.DocumentLink;

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