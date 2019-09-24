using System.ComponentModel.DataAnnotations;

namespace OPCAIC.Application.Infrastructure.Validation
{
	/// <summary>
	///     Most basic error describing class.
	/// </summary>
	public class ApplicationError
	{
		public ApplicationError()
		{

		}

		public ApplicationError(string code, string message)
		{
			Code = code;
			Message = message;
		}

		public string Code { get; }

		public string Message { get; set; }
	}
}