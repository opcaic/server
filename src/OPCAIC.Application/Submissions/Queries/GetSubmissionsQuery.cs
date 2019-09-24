using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Submissions.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Submissions.Queries
{
	public class GetSubmissionsQuery : FilterDtoBase, IRequest<PagedResult<SubmissionPreviewDto>>
	{
		public const string SortByCreated = "created";
		public const string SortByAuthor = "author";

		public long? AuthorId { get; set; }
		public bool? IsActive { get; set; }
		public long? TournamentId { get; set; }
		public long? MatchId { get; set; }
		public string Author { get; set; }
		public SubmissionValidationState? ValidationState { get; set; }

		public class Validator : FilterValidator<GetSubmissionsQuery>
		{
		}

		public class Handler
			: FilterQueryHandler<GetSubmissionsQuery, Submission, SubmissionPreviewDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, ISubmissionRepository repository) : base(mapper,
				repository)
			{
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetSubmissionsQuery request,
				ProjectingSpecification<Submission, SubmissionPreviewDto> spec)
			{
				if (request.AuthorId != null)
				{
					spec.AddCriteria(row => row.AuthorId == request.AuthorId);
				}

				if (request.Author != null)
				{
					spec.AddCriteria(row => row.Author.UserName.Contains(request.Author));
				}

				if (request.IsActive != null)
				{
					spec.AddCriteria(row
						=> row.TournamentParticipation.ActiveSubmissionId == row.Id);
				}

				if (request.TournamentId != null)
				{
					spec.AddCriteria(row => row.TournamentId == request.TournamentId);
				}

				if (request.MatchId != null)
				{
					spec.AddCriteria(row
						=> row.Participations.Any(m => m.MatchId == request.MatchId));
				}

				if (request.ValidationState != null)
				{
					spec.AddCriteria(row
						=> row.ValidationState == request.ValidationState);
				}

				spec.Ordered(GetOrderingKey(request.SortBy), request.Asc);
			}

			private Expression<Func<Submission, object>> GetOrderingKey(string key)
			{
				switch (key)
				{
					case SortByAuthor:
						return row => row.Author.UserName;
					case SortByCreated:
						return row => row.Created;
					default:
						return row => row.Id;
				}
			}
		}
	}
}