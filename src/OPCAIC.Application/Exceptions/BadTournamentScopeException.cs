using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Exceptions
{
	public class BadTournamentScopeException : BusinessException
	{
		public BadTournamentScopeException(long resourceId,
			TournamentScope expectedScope, TournamentScope actualScope) :
			base(new Error(nameof(Tournament), expectedScope.ToString(), actualScope.ToString(), resourceId))
		{
		}

		private Error MyError => (Error)base.Error;
		public string ResourceName => MyError.ResourceName;
		public string ExpectedScope => MyError.ExpectedScope;
		public string ActualScope => MyError.ActualScope;
		public long ResourceId => MyError.ResourceId;

		private new class Error : ApplicationError
		{
			/// <inheritdoc />
			public Error(string resourceName, string expectedScope, string actualScope,
				long resourceId)
				: base(ValidationErrorCodes.TournamentHasBadScope,
					$"Tournament '{resourceName}' with id {resourceId} was expected to have scope {expectedScope}, but has {actualScope} instead.")
			{
				ResourceName = resourceName;
				ExpectedScope = expectedScope;
				ActualScope = actualScope;
				ResourceId = resourceId;
			}

			public string ResourceName { get; }
			public string ExpectedScope { get; }
			public string ActualScope { get; }
			public long ResourceId { get; }
		}
	}
}