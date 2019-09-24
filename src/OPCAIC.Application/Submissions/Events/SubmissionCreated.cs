using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Submissions.Commands;

namespace OPCAIC.Application.Submissions.Events
{
	public class SubmissionCreated : INotification
	{
		/// <inheritdoc />
		public SubmissionCreated(long submissionId)
		{
			SubmissionId = submissionId;
		}

		public long SubmissionId { get; }

		public class Handler : INotificationHandler<SubmissionCreated>
		{
			private readonly IMediator mediator;

			public Handler(IMediator mediator)
			{
				this.mediator = mediator;
			}

			/// <inheritdoc />
			public Task Handle(SubmissionCreated notification,
				CancellationToken cancellationToken)
			{
				return mediator.Send(new EnqueueValidationCommand(notification.SubmissionId),
					cancellationToken);
			}
		}
	}
}