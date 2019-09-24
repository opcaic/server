using System;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Utils;

namespace OPCAIC.Application.Exceptions
{
	public class BusinessException : Exception
	{
		public BusinessException(ApplicationError error)
		{
			Require.ArgNotNull(error, nameof(error));
			Error = error;
		}

		public BusinessException(string errorCode, string message)
		{
			Error = new ApplicationError(errorCode, message);
		}

		public string ErrorCode => Error.Code;

		/// <inheritdoc />
		public override string Message => Error.Message;

		public ApplicationError Error { get; }
	}
}