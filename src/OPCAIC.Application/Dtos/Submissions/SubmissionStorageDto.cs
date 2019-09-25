using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionStorageDto : IMapFrom<Submission>
	{
		/// <summary>
		///     Submission id
		/// </summary>
		public long Id { get; set; }
	}
}