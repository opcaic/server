using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Submissions.Queries
{
	public class GetSubmissionQuery : EntityRequestQuery<Submission>, IRequest<SubmissionDetailDto>
	{
		/// <inheritdoc />
		public GetSubmissionQuery(long id) : base(id)
		{
		}

		public class Handler : EntityRequestHandler<GetSubmissionQuery, SubmissionDetailDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, ISubmissionRepository repository) : base(mapper, repository)
			{
			}
		}
	}
}