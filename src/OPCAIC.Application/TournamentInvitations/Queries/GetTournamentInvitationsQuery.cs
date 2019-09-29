using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.TournamentInvitations.Models;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.TournamentInvitations.Queries
{
	public class GetTournamentInvitationsQuery
		: FilterDtoBase, IRequest<PagedResult<TournamentInvitationDto>>
	{
		public const string SortByEmail = "email";

		public long TournamentId { get; set; }

		public class Validator : FilterValidator<GetTournamentInvitationsQuery>
		{
		}

		public class Handler
			: FilterQueryHandler<GetTournamentInvitationsQuery, TournamentInvitation,
				TournamentInvitationDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<TournamentInvitation> repository) : base(
				mapper, repository)
			{
			}

			/// <inheritdoc />
			protected override void ApplyUserFilter(ProjectingSpecification<TournamentInvitation, TournamentInvitationDto> spec, long? userId)
			{
				// only invitations to those tournaments managed by the user
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetTournamentInvitationsQuery request,
				ProjectingSpecification<TournamentInvitation, TournamentInvitationDto> spec)
			{
				spec.AddCriteria(i => i.TournamentId == request.TournamentId);
				
				switch (request.SortBy)
				{
					case SortByEmail:
						spec.Ordered(row => row.Email, request.Asc);
						break;
					default:
						spec.Ordered(row => row.Id, request.Asc);
						break;
				}
			}
		}
	}
}