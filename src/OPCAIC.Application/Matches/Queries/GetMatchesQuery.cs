using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
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
			public class MatchDetailQueryData
			{
				public static ProjectingSpecification<Match, MatchDetailQueryData> CreateSpecification(
					IMapper mapper, long? userId)
				{
					var map = mapper.GetMapExpression<Match, MatchDetailDto>();
				return ProjectingSpecification.Create(Rebind.Map((Match m) =>
					new MatchDetailQueryData
					{
						Dto = Rebind.Invoke(m, map), ShouldAnonymize = m.Tournament.Anonymize,
						CanOverrideAnonymize = m.Tournament.OwnerId == userId || m.Tournament.Managers.Any(m => m.UserId == userId)
					}
				));
				}
				public MatchDetailDto Dto { get; set; }
				public bool ShouldAnonymize { get; set; }
				public bool CanOverrideAnonymize { get; set; }
			}
	public class GetMatchesQuery : FilterDtoBase, IRequest<PagedResult<MatchDetailDto>>
	{
		public const string SortByUpdated = "updated";
		public const string SortByCreated = "created";
		public const string SortByExecuted = "executed";

		public long? TournamentId { get; set; }
		public long? UserId { get; set; }
		public long? SubmissionId { get; set; }
		public MatchState? State { get; set; }
		public string Username { get; set; }
		public bool? Anonymize { get; set; }

		public class Validator : FilterValidator<GetMatchesQuery>
		{
			public Validator()
			{
				RuleFor(m => m.State).IsInEnum();
			}
		}

		public class Handler : IRequestHandler<GetMatchesQuery, PagedResult<MatchDetailDto>>
		{
			private readonly IMapper mapper;
			private readonly IMatchRepository repository;

			/// <inheritdoc />
			public Handler(IMapper mapper, IMatchRepository repository)
			{
				this.mapper = mapper;
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<PagedResult<MatchDetailDto>> Handle(GetMatchesQuery request,
				CancellationToken cancellationToken)
			{
				var spec = MatchDetailQueryData.CreateSpecification(mapper, request.RequestingUserId);

				SetupSpecification(request, spec);
				spec.WithPaging(request.Offset, request.Count);

				if (request.RequestingUserRole != UserRole.Admin)
				{
					ApplyUserFilter(spec, request.RequestingUserId);
				}

				var result = await repository.ListPagedAsync(spec, cancellationToken);
				var toReturn = new PagedResult<MatchDetailDto>(result.Total,
					new List<MatchDetailDto>(result.Total));

				foreach (var data in result.List)
				{
					var shouldAnonymize = data.CanOverrideAnonymize
						? request.Anonymize ?? data.ShouldAnonymize
						: data.ShouldAnonymize;

					if (shouldAnonymize)
					{
						data.Dto.AnonymizeUsersExcept(request.RequestingUserId);
					}

					toReturn.List.Add(data.Dto);
				}

				return toReturn;
			}

			private void ApplyUserFilter(
				ProjectingSpecification<Match, MatchDetailQueryData> spec, long? userId)
			{
				// only matches from tournaments visible by the user (includes matches with user's submissions)
				var tournamentCriteria = GetTournamentsQuery.Handler.GetUserFilter(userId);
				spec.AddCriteria(Rebind.Map((Match m)
					=> Rebind.Invoke(m.Tournament, tournamentCriteria)));

				// also, if tournament has private matchlog, we want to hide matches of other players,
				// unless the user is tournament organizer
				spec.AddCriteria(m
					=> !m.Tournament.PrivateMatchlog ||
					m.Participations.Any(s => s.Submission.AuthorId == userId) ||
					m.Tournament.OwnerId == userId ||
					m.Tournament.Managers.Any(u => u.UserId == userId));
			}

			private void SetupSpecification(GetMatchesQuery request,
				ProjectingSpecification<Match, MatchDetailQueryData> spec)
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
						spec.AddCriteria(row => !row.LastExecution.Executed.HasValue);
						break;
					case MatchState.Executed:
						spec.AddCriteria(row
							=> row.LastExecution.ExecutorResult == EntryPointResult.Success);
						break;
					case MatchState.Failed:
						spec.AddCriteria(row
							=> row.LastExecution.ExecutorResult >= EntryPointResult.UserError);
						break;
					case MatchState.Cancelled:
						spec.AddCriteria(row
							=> row.LastExecution.State == WorkerJobState.Cancelled);
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
					case SortByExecuted:
						spec.Ordered(row => row.LastExecution.Executed ?? DateTime.MaxValue,
							request.Asc);
						break;
					default:
						spec.Ordered(row => row.Id, request.Asc);
						break;
				}
			}

		}
	}
}