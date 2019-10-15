using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentAdditionalFilesUploadedDto : IMapTo<Tournament>
	{
		public TournamentAdditionalFilesUploadedDto(bool hasAdditionalFiles)
		{
			HasAdditionalFiles = hasAdditionalFiles;
		}

		public bool HasAdditionalFiles { get; }
	}
}
