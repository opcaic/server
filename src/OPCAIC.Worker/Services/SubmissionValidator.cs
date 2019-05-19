using OPCAIC.Messaging.Messages;

namespace OPCAIC.Worker.Services
{
	class SubmissionValidator : IJobExecutor<SubmissionValidationRequest, SubmissionValidationResult> 
	{
		/// <inheritdoc />
		public SubmissionValidationResult Execute(SubmissionValidationRequest request)
		{
			return new SubmissionValidationResult()
			{
				Id = request.Id,
				Status = Status.Ok
			};
		}
	}
}