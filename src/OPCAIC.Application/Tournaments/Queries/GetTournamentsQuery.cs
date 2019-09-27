using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Application.Tournaments.Queries
{
	public class GetTournamentsQuery : FilterDtoBase, IRequest<PagedResult<TournamentPreviewDto>>
	{
		public const string SortByName = "name";
		public const string SortByCreated = "created";
		public const string SortByDeadline = "deadline";
		public const string SortByFinishedDate = "finished";
		public const string SortByStartedDate = "started";
		public const string SortByPublishedDate = "published";
		public const string SortByUserSubmissionDate = "userSubmissionDate";

		public string Name { get; set; }

		public long? GameId { get; set; }

		public long? UserId { get; set; }

		public long? OwnerId { get; set; }

		public long? ManagerId { get; set; }

		public bool? AcceptsSubmission { get; set; }

		public TournamentFormat? Format { get; set; }

		public TournamentScope? Scope { get; set; }

		public TournamentRankingStrategy? RankingStrategy { get; set; }

		public TournamentState[] State { get; set; }

		public bool? Invited { get; set; }

		public class Validator : FilterValidator<GetTournamentsQuery>
		{
			public Validator()
			{
				RuleFor(m => m.Name).MinLength(1);
				RuleFor(m => m.Format).IsInEnum();
				RuleFor(m => m.Scope).IsInEnum();
				RuleFor(m => m.RankingStrategy).IsInEnum();
				RuleFor(m => m.State).ForEach(m => m.IsInEnum());
			}
		}

		public class Handler
			: IRequestHandler<GetTournamentsQuery, PagedResult<TournamentPreviewDto>>
		{
			private readonly IMapper mapper;
			private readonly ITournamentRepository repository;

			public Handler(IMapper mapper, ITournamentRepository repository)
			{
				this.mapper = mapper;
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<PagedResult<TournamentPreviewDto>> Handle(GetTournamentsQuery request,
				CancellationToken cancellationToken)
			{
				var tournamentMap = mapper.GetMapExpression<Tournament, TournamentPreviewDto>();

				var spec = ProjectingSpecification.Create(
					request.UserId
						.HasValue // we cannot use conditionals inside the query, as the conditional would get compiled to SQL
						? Rebind.Map((Tournament t) => new TournamentPreviewAndSubmissionDate
						{
							Tournament = Rebind.Invoke(t, tournamentMap),
							LastUserSubmission = t.Participants
								.Where(p => p.ActiveSubmissionId == request.UserId)
								.Select(s => s.ActiveSubmission.Created)
								.SingleOrDefault()
						})
						: Rebind.Map((Tournament t) => new TournamentPreviewAndSubmissionDate
						{
							Tournament = Rebind.Invoke(t, tournamentMap),
							LastUserSubmission = null // we did not ask for a user
						})
				);

				// admins must be able to see everything
				if (request.RequestingUserRole != UserRole.Admin)
				{
					ApplyUserFilter(spec, request.RequestingUserId);
				}

				spec.WithPaging(request.Offset, request.Count);

				if (request.Name != null)
				{
					spec.AddCriteria(row => row.Name.ToUpper().StartsWith(request.Name.ToUpper()));
				}

				// TODO(ON): check how sql queries are generated and whether it is optimal or not (comparing nullable types instead of using .Value)
				if (request.GameId != null)
				{
					spec.AddCriteria(row => row.GameId == request.GameId);
				}

				if (request.UserId != null)
				{
					spec.AddCriteria(row => row.Participants.Any(p => p.UserId == request.UserId));
				}

				if (request.OwnerId != null)
				{
					spec.AddCriteria(row => row.OwnerId == request.OwnerId);
				}

				if (request.ManagerId != null)
				{
					spec.AddCriteria(row => row.Managers.Any(m => m.UserId == request.ManagerId));
				}

				if (request.AcceptsSubmission != null)
				{
					spec.AddCriteria(Rebind.Map((Tournament t)
						=> Rebind.Invoke(t, Tournament.AcceptsSubmissionExpression) ==
						request.AcceptsSubmission));
				}

				if (request.Format != null)
				{
					spec.AddCriteria(row => row.Format == request.Format);
				}

				if (request.Scope != null)
				{
					spec.AddCriteria(row => row.Scope == request.Scope);
				}

				if (request.RankingStrategy != null)
				{
					spec.AddCriteria(row => row.RankingStrategy == request.RankingStrategy);
				}

				if (request.State != null && request.State.Length > 0)
				{
					spec.AddCriteria(row => request.State.Contains(row.State));
				}

				if (request.Invited != null)
				{
					var userId = request.UserId ?? request.RequestingUserId;
					spec.AddCriteria(row
						=> row.Invitations.Any(i => i.UserId == userId));
				}

				AddOrdering(spec, request.SortBy, request.Asc);
				var result = await repository.ListPagedAsync(spec, cancellationToken);

				return new PagedResult<TournamentPreviewDto>(result.Total, result.List.ConvertAll(t
					=>
				{
					t.Tournament.LastUserSubmissionDate = t.LastUserSubmission;
					return t.Tournament;
				}));
			}

			public static Expression<Func<Tournament, bool>> GetUserFilter(
				long? userId)
			{
				Expression<Func<Tournament, bool>> criteria = t
					=> t.Availability == TournamentAvailability.Public;

				if (userId.HasValue)
				{
					criteria = criteria.Or(t =>
						t.Participants.Any(p => p.UserId == userId) ||
						t.Managers.Any(m => m.UserId == userId) ||
						t.OwnerId == userId);
				}

				return criteria;
			}

			private void ApplyUserFilter(
				ProjectingSpecification<Tournament, TournamentPreviewAndSubmissionDate> spec,
				long? userId)
			{
				spec.AddCriteria(GetUserFilter(userId));
			}

			private static void AddOrdering(
				ProjectingSpecification<Tournament, TournamentPreviewAndSubmissionDate> spec,
				string key,
				bool ascending)
			{
				switch (key)
				{
					case SortByCreated:
						spec.Ordered(row => row.Created, ascending);
						break;

					case SortByName:
						spec.Ordered(row => row.Name, ascending);
						break;

					case SortByDeadline:
						spec.Ordered(row => row.Deadline ?? DateTime.MaxValue, ascending);
						break;

					case SortByUserSubmissionDate:
						// put items with valid date first
						spec.OrderedProjection(row => row.LastUserSubmission == null);
						spec.OrderedProjection(row => row.LastUserSubmission, ascending);
						break;

					case SortByPublishedDate:
						spec.Ordered(row => row.Published ?? DateTime.MaxValue, ascending);
						break;

					case SortByStartedDate:
						spec.Ordered(row => row.EvaluationStarted ?? DateTime.MaxValue, ascending);
						break;

					case SortByFinishedDate:
						// treat null as DateTime.MaxValue
						spec.Ordered(row => row.EvaluationFinished ?? DateTime.MaxValue, ascending);
						break;

					default:
						spec.Ordered(row => row.Id, ascending);
						break;
				}
			}

			private class TournamentPreviewAndSubmissionDate
			{
				public TournamentPreviewDto Tournament { get; set; }
				public DateTime? LastUserSubmission { get; set; }
			}
		}
	}
}