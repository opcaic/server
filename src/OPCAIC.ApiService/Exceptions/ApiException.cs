﻿using System;

namespace OPCAIC.ApiService.Exceptions
{
	public class ApiException : Exception
	{
		public ApiException(int statusCode, string message)
			: base(message)
		{
			StatusCode = statusCode;
		}

		public int StatusCode { get; }
	}
}