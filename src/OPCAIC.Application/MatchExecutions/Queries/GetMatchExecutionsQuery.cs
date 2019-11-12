using System;
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
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Application.MatchExecutions.Queries
{
	public class GetMatchExecutionsQuery : FilterDtoBase, IAnonymize, IRequest<PagedResult<MatchExecutionPreviewDto>>
	{
		public const string SortByUpdated = "updated";
		public const string SortByCreated = "created";
		public const string SortByExecuted = "executed";

		public long? TournamentId { get; set; }
		public long? MatchId { get; set; }
		public long? UserId { get; set; }
		public long? SubmissionId { get; set; }
		public long? GameId { get; set; }
		public string Username { get; set; }
		public EntryPointResult? ExecutorResult { get; set; }
		public WorkerJobState? State { get; set; }
		public Guid? JobId { get; set; }

		/// <inheritdoc />
		public bool? Anonymize { get; set; }

		public class Validator : FilterValidator<GetMatchExecutionsQuery>
		{
			public Validator()
			{
				RuleFor(m => m.ExecutorResult).IsInEnum();
				RuleFor(m => m.State).IsInEnum();
			}
		}

		public class Handler : IRequestHandler<GetMatchExecutionsQuery, PagedResult<MatchExecutionPreviewDto>>
		{
			private readonly IMapper mapper;
			private readonly IRepository<MatchExecution> repository;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<MatchExecution> repository)
			{
				this.mapper = mapper;
				this.repository = repository;
			}

			protected void ApplyUserFilter(
				ProjectingSpecification<MatchExecution, QueryData<MatchExecutionPreviewDto>> spec,
				GetMatchExecutionsQuery request)
			{
				// only executions of managed/owned tournaments
				spec.AddCriteria(m =>
					m.Match.Tournament.OwnerId == request.RequestingUserId ||
					m.Match.Tournament.Managers.Any(u => u.UserId == request.RequestingUserId));
			}

			protected void SetupSpecification(GetMatchExecutionsQuery request,
				ProjectingSpecification<MatchExecution, QueryData<MatchExecutionPreviewDto>> spec)
			{
				if (request.TournamentId != null)
				{
					spec.AddCriteria(row => row.Match.Tournament.Id == request.TournamentId);
				}

				if (request.MatchId != null)
				{
					spec.AddCriteria(row => row.MatchId == request.MatchId);
				}

				if (request.UserId != null)
				{
					spec.AddCriteria(row
						=> row.BotResults.Any(b => b.Submission.AuthorId == request.UserId));
				}

				if (request.SubmissionId != null)
				{
					spec.AddCriteria(row =>
						row.BotResults.Any(b => b.SubmissionId == request.SubmissionId));
				}

				if (request.GameId != null)
				{
					spec.AddCriteria(row => row.Match.Tournament.GameId == request.GameId);
				}

				if (request.State != null)
				{
					spec.AddCriteria(row => row.State == request.State);
				}

				if (request.ExecutorResult != null)
				{
					spec.AddCriteria(row => row.ExecutorResult == request.ExecutorResult);
				}

				if (request.JobId != null)
				{
					spec.AddCriteria(row => row.JobId == request.JobId);
				}

				if (request.Username != null)
				{
					spec.AddCriteria(row =>
						row.BotResults.Any(s
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
						spec.Ordered(row => row.Executed ?? DateTime.MaxValue, request.Asc);
						break;
					default:
						spec.Ordered(row => row.Id, request.Asc);
						break;
				}
			}

			/// <inheritdoc />
			public async Task<PagedResult<MatchExecutionPreviewDto>> Handle(GetMatchExecutionsQuery request, CancellationToken cancellationToken)
			{
				var map = mapper.GetMapExpression<MatchExecution, MatchExecutionPreviewDto>();
				var orgMap = mapper.GetMapExpression<Tournament, TournamentOrganizersDto>();
				var spec = ProjectingSpecification.Create(Rebind.Map((MatchExecution m) =>
					new QueryData<MatchExecutionPreviewDto>
					{
						Dto = Rebind.Invoke(m, map),
						OrganizersDto = Rebind.Invoke(m.Match.Tournament, orgMap),
						TournamentAnonymize = m.Match.Tournament.Anonymize
					}
				));

				SetupSpecification(request, spec);
				spec.WithPaging(request.Offset, request.Count);

				if (request.RequestingUserRole != UserRole.Admin)
				{
					ApplyUserFilter(spec, request);
				}

				var result = await repository.ListPagedAsync(spec, cancellationToken);
				var toReturn = new PagedResult<MatchExecutionPreviewDto>(result.Total,
					new List<MatchExecutionPreviewDto>(result.Total));

				foreach (var data in result.List)
				{
					data.AnonymizeIfNecessary(request);

					toReturn.List.Add(data.Dto);
				}

				return toReturn;
			}
		}
	}
}