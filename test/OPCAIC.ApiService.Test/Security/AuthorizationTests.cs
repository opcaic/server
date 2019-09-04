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

		protected T NewTrackedEntity<T>() where T : class
		{
			var entity = EntityFaker.NewEntity<T>();
			DbContext.Set<T>().Add(entity);
			return entity;
		}

		protected User NewUser()
		{
			var user = EntityFaker.NewEntity<User>();
			GetService<UserManager>().CreateAsync(user, "pass67S#@#$@").Wait();
			return user;
		}

		protected ClaimsPrincipal ClaimsPrincipal(User user)
		{
			return GetService<SignInManager>().CreateUserPrincipalAsync(user).Result;
		}
	}
}