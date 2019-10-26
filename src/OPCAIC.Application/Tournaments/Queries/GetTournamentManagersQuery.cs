using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Tournaments.Queries
{
	public class GetTournamentManagersQuery : AuthenticatedRequest, IIdentifiedRequest, IRequest<List<string>>
	{
		/// <inheritdoc />
		public long Id { get; }

		public GetTournamentManagersQuery(long id)
		{
			Id = id;
		}

		public class Validator : AbstractValidator<GetTournamentManagersQuery>
		{
			public Validator()
			{
				RuleFor(m => m.Id).EntityId(typeof(Tournament));
			}
		}

		public class Handler : IRequestHandler<GetTournamentManagersQuery, List<string>>
		{
			private readonly IRepository<Tournament> repository;

			public Handler(IRepository<Tournament> repository)
			{
				this.repository = repository;
			}

			/// <inheritdoc />
			public Task<List<string>> Handle(GetTournamentManagersQuery request, CancellationToken cancellationToken)
			{
				return repository.FindAsync(request.Id,
					t => t.Managers.Select(m => m.User.Email).ToList(), cancellationToken);
			}
		}
	}
}