using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.Domain.Entities;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Security
{
	public abstract class AuthorizationTest : ApiServiceTestBase
	{
		private readonly Lazy<IAuthorizationService> lazyAuthorizationService;

		/// <inheritdoc />
		protected AuthorizationTest(ITestOutputHelper output) : base(output)
		{
			Services.AddAuthorization();
			Services.ConfigureSecurity(Configuration);
			lazyAuthorizationService = GetLazyService<IAuthorizationService>();
		}

		protected ClaimsPrincipal ClaimsPrincipal { get; set; }
		protected IAuthorizationService AuthorizationService => lazyAuthorizationService.Value;

		protected ClaimsPrincipal GetClaimsPrincipal(User user)
		{
			return GetService<SignInManager>().CreateUserPrincipalAsync(user).GetAwaiter()
				.GetResult();
		}
	}
}