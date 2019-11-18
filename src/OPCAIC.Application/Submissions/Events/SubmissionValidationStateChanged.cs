using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Dtos.TournamentParticipations;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Submissions.Events
{
	public class SubmissionValidationStateChanged : INotification
	{
		/// <inheritdoc />
		public SubmissionValidationStateChanged(long submissionId, long tournamentId,
			long validationId,
			long authorId, string tournamentName, SubmissionValidationState state)
		{
			SubmissionId = submissionId;
			AuthorId = authorId;
			State = state;
			TournamentId = tournamentId;
			ValidationId = validationId;
			TournamentName = tournamentName;
		}

		public long SubmissionId { get; }
		public long TournamentId { get; }
		public long ValidationId { get; }
		public long AuthorId { get; }
		public string TournamentName { get; }
		public SubmissionValidationState State { get; }

		public class Handler : INotificationHandler<SubmissionValidationStateChanged>
		{
			private readonly IRepository<TournamentParticipation> participationsRepository;
			private readonly IRepository<Submission> repository;

			/// <inheritdoc />
			public Handler(IRepository<Submission> repository,
				IRepository<TournamentParticipation> participationsRepository)
			{
				this.repository = repository;
				this.participationsRepository = participationsRepository;
			}

			/// <inheritdoc />
			public async Task Handle(SubmissionValidationStateChanged notification,
				CancellationToken cancellationToken)
			{
				if (notification.State != SubmissionValidationState.Valid)
				{
					return;
				}

				var data = await repository.GetAsync(notification.SubmissionId, s => new Data
				{
					LastSubmissionId = s.TournamentParticipation.Submissions
						.OrderByDescending(ss => ss.Created).First().Id
				}, cancellationToken);

				if (notification.SubmissionId != data.LastSubmissionId)
				{
					return; // newer submission exists
				}

				var participation = await participationsRepository.GetAsync(p => 
						p.TournamentId == notification.TournamentId &&
						p.UserId == notification.AuthorId,
					cancellationToken);
				participation.ActiveSubmissionId = notification.SubmissionId;

				await participationsRepository.SaveChangesAsync(cancellationToken);
			}

			public class Data
			{
				public long LastSubmissionId { get; set; }
			}
		}
	}
}