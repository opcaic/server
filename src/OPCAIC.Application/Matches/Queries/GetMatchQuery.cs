using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Matches.Models;

namespace OPCAIC.Application.Matches.Queries
{
	public class GetMatchQuery : AuthenticatedRequest, IRequest<MatchDetailDto>
	{
		/// <inheritdoc />
		public GetMatchQuery(long id)
		{
			Id = id;
		}

		public long Id { get; set; }

		public bool? Anonymize { get; set; }

		public class Handler : IRequestHandler<GetMatchQuery, MatchDetailDto>
		{
			private readonly IMapper mapper;
			private readonly IMatchRepository repository;

			public Handler(IMapper mapper, IMatchRepository repository)
			{
				this.mapper = mapper;
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<MatchDetailDto> Handle(GetMatchQuery request, CancellationToken cancellationToken)
			{
				var spec = MatchDetailQueryData.CreateSpecification(mapper, request.RequestingUserId);
				spec.AddCriteria(m => m.Id == request.Id);

				var data = await repository.GetAsync(spec, cancellationToken);

				var shouldAnonymize = data.CanOverrideAnonymize
					? request.Anonymize ?? data.ShouldAnonymize
					: data.ShouldAnonymize;

				if (shouldAnonymize)
				{
					data.Dto.AnonymizeUsersExcept(request.RequestingUserId);
				}

				return data.Dto;
			}
		}
	}
}