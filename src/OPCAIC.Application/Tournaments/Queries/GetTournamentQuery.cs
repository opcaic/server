using AutoMapper;
using MediatR;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Tournaments.Queries
{
	public class GetTournamentQuery : EntityRequestQuery<Tournament>, IRequest<TournamentDetailDto>
	{
		/// <inheritdoc />
		public GetTournamentQuery(long id) : base(id)
		{
		}

		public class Handler : EntityRequestHandler<GetTournamentQuery, TournamentDetailDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Tournament> repository) : base(mapper,
				repository)
			{
			}
		}
	}
}