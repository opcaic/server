using System;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Exceptions
{
	public class ApiException : Exception
	{
		public ApiException(int statusCode, string message, string code)
			: base(message)
		{
			Require.ArgNotNull(message, "Api exception message must not be null");

			StatusCode = statusCode;
			Code = code;
		}

		public int StatusCode { get; }

		public string Code { get; }
	}
}