using OPCAIC.Domain.ValueObjects;

namespace OPCAIC.Application.Tournaments.Models
{
	public class ExternalUrlMenuItemDto : MenuItemDto
	{
		public string Text { get; set; }
		public string ExternalLink { get; set; }
	}
}