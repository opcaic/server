using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace OPCAIC.ApiService.Exceptions
{
	/// <summary>
	///     Represents an error from Identity framework operation.
	/// </summary>
	[Serializable]
	public class IdentityErrorException : InvalidOperationException
	{
		public IdentityErrorException(IEnumerable<IdentityError> identityErrors) : base(
			"Operation resulted in failed IdentityResult.")
		{
			IdentityErrors = identityErrors;
		}

		/// <summary>
		///     Errors obtained from the failed <see cref="IdentityResult"/>.
		/// </summary>
		public IEnumerable<IdentityError> IdentityErrors { get; }

		/// <inheritdoc />
		public override string ToString()
		{
			return base.ToString() +
				"\nErrors:\n" +
				string.Concat("\n", IdentityErrors.Select(e => $"[{e.Code}] - {e.Description}"));
		}
	}
}