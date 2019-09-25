using System;
using System.Linq;
using AutoMapper;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Queries;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Application.Matches.Queries
{
	public class GetMatchesQuery : FilterDtoBase, IRequest<PagedResult<MatchDetailDto>>
	{
		public const string SortByUpdated = "updated";
		public const string SortByCreated = "created";

		public long? TournamentId { get; set; }
		public long? UserId { get; set; }
		public long? SubmissionId { get; set; }
		public MatchState? State { get; set; }
		public string Username { get; set; }

		public class Validator : FilterValidator<GetMatchesQuery>
		{
			public Validator()
			{
				RuleFor(m => m.State).IsInEnum();
			}
		}

		public class Handler : FilterQueryHandler<GetMatchesQuery, Match, MatchDetailDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IMatchRepository repository) : base(mapper, repository)
			{
			}

			/// <inheritdoc />
			protected override void ApplyUserFilter(ProjectingSpecification<Match, MatchDetailDto> spec, long? userId)
			{
				// only matches from tournaments visible by the user (includes matches with user's submissions)
				var tournamentCriteria = GetTournamentsQuery.Handler.GetUserFilter(userId);
				spec.AddCriteria(Rebind.Map((Match m)
					=> Rebind.Invoke(m.Tournament, tournamentCriteria)));
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetMatchesQuery request,
				ProjectingSpecification<Match, MatchDetailDto> spec)
			{
				if (request.TournamentId != null)
				{
					spec.AddCriteria(row => row.Tournament.Id == request.TournamentId);
				}

				if (request.UserId != null)
				{
					spec.AddCriteria(row
						=> row.Participations.Any(p => p.Submission.AuthorId == request.UserId));
				}

				if (request.SubmissionId != null)
				{
					spec.AddCriteria(row =>
						row.Participations.Any(p => p.SubmissionId == request.SubmissionId));
				}

				switch (request.State)
				{
					// a match should always have at least one execution
					case null:
						break; // nothing
					case MatchState.Queued:
						spec.AddCriteria(row => !row.Executions
							.OrderByDescending(e => e.Created).First().Executed.HasValue);
						break;
					case MatchState.Executed:
						spec.AddCriteria(row => row.Executions
								.OrderByDescending(e => e.Created).First().ExecutorResult ==
							EntryPointResult.Success);
						break;
					case MatchState.Failed:
						spec.AddCriteria(row => row.Executions
								.OrderByDescending(e => e.Created).First().ExecutorResult >=
							EntryPointResult.UserError);
						break;
					case MatchState.Cancelled:
						spec.AddCriteria(row
							=> row.Executions.OrderByDescending(e => e.Created).First().State ==
							WorkerJobState.Cancelled);
						break;
					default:

						throw new ArgumentOutOfRangeException();
				}

				if (request.Username != null)
				{
					spec.AddCriteria(row =>
						row.Participations.Any(s
							=> s.Submission.Author.UserName.Contains(request.Username)));
				}

				switch (request.SortBy)
				{
					case SortByCreated:
						spec.Ordered(row => row.Created, request.Asc);
						break;
					case SortByUpdated:
						spec.Ordered(row => row.Updated, request.Asc);
						break;
					default:
						spec.Ordered(row => row.Id, request.Asc);
						break;
				}
			}
		}
	}
}