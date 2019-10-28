using System.Collections.Generic;
using OPCAIC.Domain.ValueObjects;

namespace OPCAIC.Application.Tournaments.Models
{
	public class ExternalUrlMenuItemDto : MenuItemDto
	{
		public string Text { get; set; }
		public string ExternalLink { get; set; }

		/// <inheritdoc />
		protected override IEnumerable<object> GetAtomicValues()
		{
			yield return Text;
			yield return ExternalLink;
		}
	}
}