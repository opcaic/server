using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Exceptions
{
	class PublishedTournamentUpdateException : BusinessException
	{
		public PublishedTournamentUpdateException(long resourceId,
			string propertyName) :
			base(new Error(nameof(Tournament), propertyName, resourceId))
		{
		}

		private Error MyError => (Error)base.Error;
		public string ResourceName => MyError.ResourceName;
		public string PropertyName => MyError.PropertyName;
		public long ResourceId => MyError.ResourceId;

		private new class Error : ApplicationError
		{
			/// <inheritdoc />
			public Error(string resourceName, string propertyName,
				long resourceId) : base(ValidationErrorCodes.PublishedTournamentBadUpdate,
				$"Tournament '{resourceName}' with id {resourceId} is published, and therefore its property {propertyName} cannot be updated.")
			{
				ResourceName = resourceName;
				PropertyName = propertyName;
				ResourceId = resourceId;
			}

			public string ResourceName { get; }
			public string PropertyName { get; }
			public long ResourceId { get; }
		}
	}
}