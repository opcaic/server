using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Services
{
	public class SubmissionService : ISubmissionService
	{
		private readonly ISubmissionRepository repository;
		private readonly IStorageService storage;

		public SubmissionService(ISubmissionRepository repository, IStorageService storage)
		{
			this.repository = repository;
			this.storage = storage;
		}

		public async Task<Stream> GetSubmissionArchiveAsync(long id, CancellationToken cancellationToken)
		{
			var dto = await repository.FindSubmissionForStorageAsync(id, cancellationToken);

			if (dto == null)
			{
				throw new NotFoundException(nameof(Submission), id);
			}

			return storage.ReadSubmissionArchive(dto);
		}

		/// <inheritdoc />
		public Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken)
		{
			return repository.ExistsByIdAsync(id, cancellationToken);
		}
	}
}