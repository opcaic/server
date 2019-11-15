using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Specifications;
using OPCAIC.Broker;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class DeleteTournamentCommand : IRequest
	{
		public DeleteTournamentCommand(long tournamentId)
		{
			TournamentId = tournamentId;
		}

		public long TournamentId { get; }

		public class Handler : IRequestHandler<DeleteTournamentCommand>
		{
			private readonly IBroker broker;
			private readonly IRepository<Tournament> repository;
			private readonly IStorageService storage;

			/// <inheritdoc />
			public Handler(IRepository<Tournament> repository, IStorageService storage,
				IBroker broker)
			{
				this.repository = repository;
				this.storage = storage;
				this.broker = broker;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(DeleteTournamentCommand request,
				CancellationToken cancellationToken)
			{
				var data = await repository.GetAsync(request.TournamentId,
					t => new Data
					{
						Submissions = t.Submissions.Select(s => new Data.SubmissionDto
						{
							Id = s.Id,
							Validations = s.Validations
								.Where(s => s.State == WorkerJobState.Scheduled)
								.Select(v => v.JobId).ToList()
						}).ToList(),
						Matches = t.Matches.Select(m => new Data.MatchDto
						{
							Id = m.Id,
							Executions = m.Executions
								.Where(e => e.State == WorkerJobState.Scheduled)
								.Select(e => e.JobId).ToList()
						}).ToList(),
						State = t.State
					}, cancellationToken);

				// cancel all pending requests
				await Task.WhenAll(data.Matches.SelectMany(m => m.Executions)
					.Select(broker.CancelWork).Concat(data.Submissions
						.SelectMany(s => s.Validations).Select(broker.CancelWork)));

				await repository.DeleteAsync(request.TournamentId, cancellationToken);
				storage.DeleteAllTournamentFiles(request.TournamentId);

				return Unit.Value;
			}

			public class Data
			{
				public List<SubmissionDto> Submissions { get; set; }
				public List<MatchDto> Matches { get; set; }

				public TournamentState State { get; set; }

				public class MatchDto
				{
					public long Id { get; set; }
					public List<Guid> Executions { get; set; }
				}

				public class SubmissionDto
				{
					public long Id { get; set; }
					public List<Guid> Validations { get; set; }
				}
			}
		}
	}
}