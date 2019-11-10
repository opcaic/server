using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Specifications;
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
			private readonly IRepository<Tournament> repository;
			private readonly IStorageService storage;

			public class Data
			{
				public class MatchDto
				{
					public long Id { get; set; }
					public List<long> Executions { get; set; }
				}

				public class SubmissionDto
				{
					public long Id { get; set; }
					public List<long> Validations { get; set; }
				}

				public List<SubmissionDto> Submissions { get; set; }
				public List<MatchDto> Matches { get; set; }

				public TournamentState State { get; set; }
			}

			/// <inheritdoc />
			public Handler(IRepository<Tournament> repository, IStorageService storage)
			{
				this.repository = repository;
				this.storage = storage;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(DeleteTournamentCommand request, CancellationToken cancellationToken)
			{
				var data = await repository.GetAsync(request.TournamentId, (Tournament t) => new Data
				{
					Submissions = t.Submissions.Select(s => new Data.SubmissionDto
					{
						Id = s.Id,
						Validations = s.Validations.Select(v => v.Id).ToList()
					}).ToList(),
					Matches = t.Matches.Select(m => new Data.MatchDto { Id = m.Id, Executions = m.Executions.Select(e => e.Id).ToList() }).ToList(),
					State = t.State
				}, cancellationToken);

				if (data.State != TournamentState.Finished)
				{
					throw new BadTournamentStateException(request.TournamentId, TournamentState.Finished, data.State);
				}

				await repository.DeleteAsync(request.TournamentId, cancellationToken);
				storage.DeleteAllTournamentFiles(request.TournamentId);

				return Unit.Value;
			}
		}
	}
}