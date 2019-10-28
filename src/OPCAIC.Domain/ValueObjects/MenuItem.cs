using System;
using System.Collections.Generic;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.Domain.ValueObjects
{
	public abstract class MenuItem : ValueObject
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

		/// <inheritdoc />
		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Type;
			yield return Text;
			yield return ExternalLink;
		}
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

		/// <inheritdoc />
		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Type;
			yield return DocumentId;
		}
	}

	public enum MenuItemType
	{
		DocumentLink,
		ExternalUrl
	}
}