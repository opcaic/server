using System;
using System.Collections.Generic;
using System.Security.Claims;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Security.Handlers;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Security
{
	public abstract class AuthorizationTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		protected AuthorizationTest(ITestOutputHelper output) : base(output)
		{
		}

		protected ClaimsPrincipal ClaimsPrincipal(User user)
		{
			return GetService<SignInManager>().CreateUserPrincipalAsync(user).Result;
		}
	}
}