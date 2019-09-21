using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Exceptions;

namespace OPCAIC.Application.Matches.Queries
{
	public class GetMatchQuery : IRequest<MatchDetailDto>
	{
		/// <inheritdoc />
		public GetMatchQuery(long id)
		{
			Id = id;
		}

		public long Id { get; }

		public class Handler : IRequestHandler<GetMatchQuery, MatchDetailDto>
		{
			private readonly IMatchRepository repository;

			public Handler(IMatchRepository repository)
			{
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<MatchDetailDto> Handle(GetMatchQuery request, CancellationToken cancellationToken)
			{
				var dto = await repository.FindByIdAsync(request.Id, cancellationToken);

				if (dto == null)
				{
					throw new NotFoundException(nameof(Match), request.Id);
				}

				return dto;
			}
		}
	}
}