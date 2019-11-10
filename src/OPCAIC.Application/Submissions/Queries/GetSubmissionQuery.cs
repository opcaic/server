using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Submissions.Queries
{
	public class GetSubmissionQuery : EntityRequestQuery<Submission>, IRequest<SubmissionDetailDto>
	{
		/// <inheritdoc />
		public GetSubmissionQuery(long id) : base(id)
		{
		}

		public class Handler : EntityRequestHandler<GetSubmissionQuery, SubmissionDetailDto>
		{
			private readonly ILogStorageService logStorage;

			/// <inheritdoc />
			public Handler(IMapper mapper, ISubmissionRepository repository, ILogStorageService logStorage) : base(mapper, repository)
			{
				this.logStorage = logStorage;
			}

			/// <inheritdoc />
			public override async Task<SubmissionDetailDto> Handle(GetSubmissionQuery request, CancellationToken cancellationToken)
			{
				var dto = await base.Handle(request, cancellationToken);

				dto.LastValidation?.AddLogs(logStorage.GetSubmissionValidationLogs(dto.LastValidation));

				return dto;
			}
		}
	}

	public class GetSubmissionAdminQuery : EntityRequestQuery<Submission>, IRequest<SubmissionAdminDto>
	{
		/// <inheritdoc />
		public GetSubmissionAdminQuery(long id) : base(id)
		{
		}

		public class Handler : EntityRequestHandler<GetSubmissionAdminQuery, SubmissionAdminDto>
		{
			private readonly ILogStorageService logStorage;

			/// <inheritdoc />
			public Handler(IMapper mapper, ISubmissionRepository repository, ILogStorageService logStorage) : base(mapper, repository)
			{
				this.logStorage = logStorage;
			}

			/// <inheritdoc />
			public override async Task<SubmissionAdminDto> Handle(GetSubmissionAdminQuery request, CancellationToken cancellationToken)
			{
				var dto = await base.Handle(request, cancellationToken);

				foreach (var validation in dto.Validations)
				{
					validation.AddLogs(logStorage.GetSubmissionValidationLogs(validation));
				}

				return dto;
			}
		}
	}
}