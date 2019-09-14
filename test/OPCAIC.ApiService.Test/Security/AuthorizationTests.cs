using System;
using System.Security.Claims;
using System.Threading;
using Bogus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Security.Handlers;
using OPCAIC.ApiService.Services;
using OPCAIC.Domain.Entities;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Repositories;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Security
{
	public abstract class AuthorizationTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		protected AuthorizationTest(ITestOutputHelper output) : base(output)
		{
			Services.AddAuthorization();
			Services.ConfigureSecurity(Configuration);
			Services.AddMapper();
			Services.AddRepositories();
			UseDatabase();

			lazyAuthorizationService = GetLazyService<IAuthorizationService>();
		}

		protected ClaimsPrincipal GetClaimsPrincipal(User user)
		{
			return GetService<SignInManager>().CreateUserPrincipalAsync(user).Result;
		}

		protected ClaimsPrincipal ClaimsPrincipal { get; set; }

		private readonly Lazy<IAuthorizationService> lazyAuthorizationService;
		protected IAuthorizationService AuthorizationService => lazyAuthorizationService.Value;
	}
}