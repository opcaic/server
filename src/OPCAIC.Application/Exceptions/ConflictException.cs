using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.Application.Exceptions
{
	public class ConflictException : BusinessException
	{
		public string Field => ((ConflictError)Error).Field;

		public ConflictException(string code, string field, string message = null)
			: base(new ConflictError(code, message, field))
		{
		}

		public class ConflictError : ApplicationError
		{
			/// <inheritdoc />
			public ConflictError(string code, string message, string field) : base(code, message)
			{
				Field = field;
			}

			public string Field { get; }
		}
	}
}