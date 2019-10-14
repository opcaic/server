using System;
using System.Linq;
using AutoMapper;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.SubmissionValidations
{
	public class GetSubmissionValidationsQuery
		: FilterDtoBase, IRequest<PagedResult<SubmissionValidationDto>>
	{
		public const string SortByUpdated = "updated";
		public const string SortByCreated = "created";
		public const string SortByExecuted = "executed";

		public long? SubmissionId { get; set; }
		public long? TournamentId { get; set; }
		public long? UserId { get; set; }
		public long? GameId { get; set; }
		public string Username { get; set; }
		public Guid? JobId { get; set; }
		public EntryPointResult? CheckerResult { get; set; }
		public EntryPointResult? CompilerResult { get; set; }
		public EntryPointResult? ValidatorResult { get; set; }

		public class Validator : FilterValidator<GetSubmissionValidationsQuery>
		{
			/// <inheritdoc />
			public Validator()
			{
				RuleFor(s => s.CheckerResult).IsInEnum();
				RuleFor(s => s.CompilerResult).IsInEnum();
				RuleFor(s => s.ValidatorResult).IsInEnum();
			}
		}

		public class Handler
			: FilterQueryHandler<GetSubmissionValidationsQuery, SubmissionValidation,
				SubmissionValidationDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<SubmissionValidation> repository) : base(
				mapper, repository)
			{
			}

			/// <inheritdoc />
			protected override void ApplyUserFilter(
				ProjectingSpecification<SubmissionValidation, SubmissionValidationDto> spec,
				long? userId)
			{
				// only executions of managed/owned tournaments
				spec.AddCriteria(submissionValidation =>
					submissionValidation.Submission.Tournament.OwnerId == userId ||
					submissionValidation.Submission.Tournament.Managers.Any(u
						=> u.UserId == userId));
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetSubmissionValidationsQuery request,
				ProjectingSpecification<SubmissionValidation, SubmissionValidationDto> spec)
			{
				if (request.TournamentId != null)
				{
					spec.AddCriteria(row => row.Submission.Tournament.Id == request.TournamentId);
				}

				if (request.SubmissionId != null)
				{
					spec.AddCriteria(row => row.SubmissionId == request.SubmissionId);
				}

				if (request.UserId != null)
				{
					spec.AddCriteria(row
						=> row.Submission.AuthorId == request.UserId);
				}

				if (request.GameId != null)
				{
					spec.AddCriteria(row => row.Submission.Tournament.GameId == request.GameId);
				}

				if (request.CheckerResult != null)
				{
					spec.AddCriteria(row => row.CheckerResult == request.CheckerResult);
				}

				if (request.ValidatorResult != null)
				{
					spec.AddCriteria(row => row.ValidatorResult == request.ValidatorResult);
				}

				if (request.CompilerResult != null)
				{
					spec.AddCriteria(row => row.CompilerResult == request.CompilerResult);
				}

				if (request.JobId != null)
				{
					spec.AddCriteria(row => row.JobId == request.JobId);
				}

				if (request.Username != null)
				{
					spec.AddCriteria(row =>
						row.Submission.Author.UserName.Contains(request.Username));
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
		}
	}
}