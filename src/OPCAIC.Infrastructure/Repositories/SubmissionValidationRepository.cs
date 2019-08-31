using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class SubmissionValidationRepository : RepositoryBase<SubmissionValidation>, ISubmissionValidationRepository
	{
		/// <inheritdoc />
		public SubmissionValidationRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<long> CreateAsync(NewSubmissionValidationDto dto, CancellationToken cancellationToken)
		{
			return CreateFromDtoAsync(dto, cancellationToken);
		}

		/// <inheritdoc />
		public Task<SubmissionValidationStorageDto> FindStorageAsync(long id, CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<SubmissionValidationStorageDto>(id, cancellationToken);
		}

		public Task<bool> UpdateFromJobAsync(Guid jobId, UpdateSubmissionValidationDto dto,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoByQueryAsync(v => v.JobId == jobId, dto, cancellationToken);
		}
	}
}