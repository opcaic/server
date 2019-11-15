using AutoMapper;
using Newtonsoft.Json.Linq;

namespace OPCAIC.Application.Tournaments.Models
{
	public class TournamentAdminDto : TournamentDetailDto
	{
		[IgnoreMap]
		public long? AdditionalFilesLength { get; set; }

		public JObject Configuration { get; set; }
	}
}