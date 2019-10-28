using System.Collections.Generic;
using OPCAIC.Domain.ValueObjects;

namespace OPCAIC.Application.Tournaments.Models
{
	public class DocumentLinkMenuItemDto : MenuItemDto
	{
		public long DocumentId { get; set; }

		/// <inheritdoc />
		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return DocumentId;
		}
	}
}