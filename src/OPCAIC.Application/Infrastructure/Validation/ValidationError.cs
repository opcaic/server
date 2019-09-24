namespace OPCAIC.Application.Infrastructure.Validation
{
	/// <summary>
	///     Basic validation error that uses only the minimum required information.
	/// </summary>
	public class ValidationError : ApplicationError
	{
		/// <inheritdoc />
		public ValidationError(string code, string message, string field) : base(code, message)
		{
			Field = field;
		}

		public string Field { get; set; }
	}
}