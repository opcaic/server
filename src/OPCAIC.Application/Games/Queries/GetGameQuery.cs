using AutoMapper;
using MediatR;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Games.Queries
{
	public class GetGameQuery : EntityRequestQuery<Game>, IRequest<GameDetailDto>
	{
		/// <inheritdoc />
		public GetGameQuery(long id) : base(id)
		{
		}

		public class Handler : EntityRequestHandler<GetGameQuery, GameDetailDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Game> repository) : base(mapper, repository)
			{
			}
		}
	}
}