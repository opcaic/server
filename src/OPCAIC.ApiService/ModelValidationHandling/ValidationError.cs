namespace OPCAIC.ApiService.ModelValidationHandling
{
	/// <summary>
	/// Basic validation error that uses only the minimum required information.
	/// </summary>
	public class ValidationError : ValidationErrorBase
	{
		/// <inheritdoc />
		public ValidationError(string code, string message, string field) : base(code, message)
		{
			Field = field;
		}
	}
}