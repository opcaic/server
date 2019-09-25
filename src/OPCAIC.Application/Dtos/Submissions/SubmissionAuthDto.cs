using System.Linq;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionAuthDto : ICustomMapping
	{
		public long Id { get; set; }
		public long AuthorId { get; set; }
		public long TournamentOwnerId { get; set; }
		public long[] TournamentManagersIds { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<Submission, SubmissionAuthDto>(MemberList.Destination)
				.ForMember(d => d.TournamentManagersIds,
					opt => opt.MapFrom(s => s.Tournament.Managers.Select(m => m.UserId)));
		}
	}
}