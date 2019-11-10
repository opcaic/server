using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.SubmissionValidations.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Utils;

namespace OPCAIC.Application.SubmissionValidations.Queries
{
	public class GetSubmissionValidationQuery
		: GetSubmissionValidationQueryBase, IRequest<SubmissionValidationDetailDto>
	{
		/// <inheritdoc />
		public GetSubmissionValidationQuery(long validationId) : base(validationId)
		{
		}

		public class Handler : HandlerBase<GetSubmissionValidationQuery, SubmissionValidationDetailDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<SubmissionValidation> repository, ILogStorageService logStorage) : base(mapper, repository, logStorage)
			{
			}
		}
	}

	public class GetSubmissionValidationAdminQuery
		: GetSubmissionValidationQueryBase, IRequest<SubmissionValidationAdminDto>
	{
		/// <inheritdoc />
		public GetSubmissionValidationAdminQuery(long validationId) : base(validationId)
		{
		}

		public class Handler : HandlerBase<GetSubmissionValidationAdminQuery, SubmissionValidationAdminDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<SubmissionValidation> repository, ILogStorageService logStorage) : base(mapper, repository, logStorage)
			{
			}
		}
	}

	public class GetSubmissionValidationQueryBase
	{
		/// <inheritdoc />
		public GetSubmissionValidationQueryBase(long validationId)
		{
			ValidationId = validationId;
		}

		public long ValidationId { get; }

		public class HandlerBase<TRequest, TResponse>
			: IRequestHandler<TRequest, TResponse>
			where TRequest : GetSubmissionValidationQueryBase, IRequest<TResponse>
			where TResponse : SubmissionValidationDetailDto
		{
			private readonly ILogStorageService logStorage;
			private readonly IMapper mapper;
			private readonly IRepository<SubmissionValidation> repository;

			/// <inheritdoc />
			public HandlerBase(IMapper mapper, IRepository<SubmissionValidation> repository,
				ILogStorageService logStorage)
			{
				this.mapper = mapper;
				this.repository = repository;
				this.logStorage = logStorage;
			}

			/// <inheritdoc />
			public async Task<TResponse> Handle(
				TRequest request, CancellationToken cancellationToken)
			{
				var dto = await repository
					.GetAsync<SubmissionValidation, TResponse>(request.ValidationId, mapper, cancellationToken);

				var logs = logStorage.GetSubmissionValidationLogs(dto);

				dto.AddLogs(logs);
				return dto;
			}
		}
	}
}