﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Extensions;
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
	public class GetMatchesQuery : FilterDtoBase, IAnonymize, IRequest<PagedResult<MatchPreviewDto>>
	{
		public const string SortByUpdated = "updated";
		public const string SortByCreated = "created";
		public const string SortByExecuted = "executed";

		public bool ManagedOnly { get; set; }
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

		public class Handler : IRequestHandler<GetMatchesQuery, PagedResult<MatchPreviewDto>>
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
			public async Task<PagedResult<MatchPreviewDto>> Handle(GetMatchesQuery request,
				CancellationToken cancellationToken)
			{
				var map = mapper.GetMapExpression<Match, MatchPreviewDto>();
				var orgMap = mapper.GetMapExpression<Tournament, TournamentOrganizersDto>();
				var spec = ProjectingSpecification.Create(Rebind.Map((Match m) =>
					new QueryData<MatchPreviewDto>
					{
						Dto = Rebind.Invoke(m, map),
						OrganizersDto = Rebind.Invoke(m.Tournament, orgMap),
						TournamentAnonymize = m.Tournament.Anonymize
					}
				));

				SetupSpecification(request, spec);
				spec.WithPaging(request.Offset, request.Count);

				if (request.RequestingUserRole != UserRole.Admin)
				{
					ApplyUserFilter(spec, request.RequestingUserId, request.ManagedOnly);
				}

				var result = await repository.ListPagedAsync(spec, cancellationToken);
				var toReturn = new PagedResult<MatchPreviewDto>(result.Total,
					new List<MatchPreviewDto>(result.Total));

				foreach (var data in result.List)
				{
					data.AnonymizeIfNecessary(request);

					toReturn.List.Add(data.Dto);
				}

				return toReturn;
			}

			private void ApplyUserFilter(
				ProjectingSpecification<Match, QueryData<MatchPreviewDto>> spec, long? userId, bool managedOnly)
			{
				// only matches from tournaments visible by the user (includes matches with user's submissions)
				var tournamentCriteria = GetTournamentsQuery.Handler.GetUserFilter(userId, managedOnly);
				spec.AddCriteria(Rebind.Map((Match m)
					=> Rebind.Invoke(m.Tournament, tournamentCriteria)));

				if (!managedOnly)
				{
					// also, if tournament has private matchlog, we want to hide matches of other players,
					// unless the user is tournament organizer
					spec.AddCriteria(m
						=> !m.Tournament.PrivateMatchlog ||
						m.Participations.Any(s => s.Submission.AuthorId == userId) ||
						m.Tournament.OwnerId == userId ||
						m.Tournament.Managers.Any(u => u.UserId == userId));
				}
			}

			private void SetupSpecification(GetMatchesQuery request,
				ProjectingSpecification<Match, QueryData<MatchPreviewDto>> spec)
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