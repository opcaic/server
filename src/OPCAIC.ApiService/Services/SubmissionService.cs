using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Services
{
	public class SubmissionService : ISubmissionService
	{
		private readonly IMapper mapper;
		private readonly ISubmissionRepository repository;
		private readonly ITournamentRepository tournamentRepository;
		private readonly IStorageService storage;
		private readonly ILogger<SubmissionService> logger;

		public SubmissionService(IMapper mapper, ISubmissionRepository repository, IStorageService storage, ITournamentRepository tournamentRepository, ILogger<SubmissionService> logger)
		{
			this.repository = repository;
			this.storage = storage;
			this.tournamentRepository = tournamentRepository;
			this.logger = logger;
			this.mapper = mapper;
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