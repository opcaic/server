using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

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
			public Task<PagedResult<MatchDetailDto>> Handle(GetMatchesQuery request,
				CancellationToken cancellationToken)
			{
				var spec = ProjectingSpecification<Match>.Create<MatchDetailDto>(mapper);
				spec.WithPaging(request.Offset, request.Count);


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

				return repository.ListPagedAsync(spec, cancellationToken);
			}
		}
	}
}