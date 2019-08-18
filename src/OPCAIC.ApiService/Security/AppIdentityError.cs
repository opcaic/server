using Microsoft.AspNetCore.Identity;
using OPCAIC.ApiService.ModelValidationHandling;

namespace OPCAIC.ApiService.Security
{
	public class AppIdentityError : IdentityError
	{
		public ValidationErrorBase ValidationError { get; set; }
	}
}