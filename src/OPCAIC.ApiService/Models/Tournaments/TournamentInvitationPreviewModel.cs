using Newtonsoft.Json;
using OPCAIC.Application.Dtos.Users;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class TournamentInvitationPreviewModel
	{
		public long Id { get; set; }

		public string Email { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public UserReferenceDto User { get; set; }
	}
}
