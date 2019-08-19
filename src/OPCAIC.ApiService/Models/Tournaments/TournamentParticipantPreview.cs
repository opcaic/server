using Newtonsoft.Json;
using OPCAIC.Infrastructure.Dtos.Users;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class TournamentParticipantPreview
	{
		public long Id { get; set; }

		public string Email { get; set; }

		[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
		public UserReferenceDto User { get; set; }
	}
}
