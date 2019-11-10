using System;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Exceptions
{
	[Serializable]
	public class BadTournamentStateException : BusinessException
	{
		public BadTournamentStateException(long resourceId,
			TournamentState expectedState, TournamentState actualState) :
			base(new Error(nameof(Tournament), expectedState.ToString(), actualState.ToString(), resourceId))
		{
		}

		private Error MyError => (Error)base.Error;
		public string ResourceName => MyError.ResourceName;
		public string ExpectedState => MyError.ExpectedState;
		public string ActualState => MyError.ActualState;
		public long ResourceId => MyError.ResourceId;

		private new class Error : ApplicationError
		{
			/// <inheritdoc />
			public Error(string resourceName, string expectedState, string actualState,
				long resourceId) : base(ValidationErrorCodes.TournamentInBadState,
				$"Tournament '{resourceName}' with id {resourceId} was expected to have state {expectedState}, but has {actualState} instead.")
			{
				ResourceName = resourceName;
				ExpectedState = expectedState;
				ActualState = actualState;
				ResourceId = resourceId;
			}

			public string ResourceName { get; }
			public string ExpectedState { get; }
			public string ActualState { get; }
			public long ResourceId { get; }
		}
	}
}