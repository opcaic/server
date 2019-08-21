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
		private readonly Dictionary<Type, object> fakers = new Dictionary<Type, object>();

		/// <inheritdoc />
		protected AuthorizationTest(ITestOutputHelper output) : base(output)
		{

			AddFakers();
		}

		private void AddFakers()
		{
			AddFaker(new Faker<User>()
				.UseSeed(10)
				.RuleFor(u => u.FirstName, f => f.Name.FirstName())
				.RuleFor(u => u.LastName, f => f.Name.LastName())
				.RuleFor(u => u.UserName, f => f.Name.Random.String(8))
				.RuleFor(u => u.Email, f => f.Internet.Email()));

			AddFaker(new Faker<Game>()
				.UseSeed(10)
				.RuleFor(g => g.Name, f => f.Random.String(10))
				.RuleFor(g => g.Description, f => f.Random.String())
				.RuleFor(g => g.GameType, f => f.PickRandom<GameType>()));

			AddFaker(new Faker<Tournament>()
				.UseSeed(10)
				.RuleFor(g => g.Name, f => f.Random.String(10))
				.RuleFor(g => g.Description, f => f.Random.String())
				.RuleFor(g => g.Availability,
					(f, g) => g.Availability = TournamentAvailability.Public));
		}

		private void AddFaker<T>(Faker<T> faker) where T : class
		{
			fakers.Add(typeof(T), faker);
		}


		protected T NewTrackedEntity<T>() where T : class
		{
			var entity = NewEntity<T>();
			DbContext.Set<T>().Add(entity);
			return entity;
		}

		private T NewEntity<T>() where T : class
		{
			return ((Faker<T>) fakers[typeof(T)]).Generate();
		}

		protected User NewUser()
		{
			var user = NewEntity<User>();
			GetService<UserManager>().CreateAsync(user, "pass67S#@#$@").Wait();
			return user;
		}

		protected ClaimsPrincipal ClaimsPrincipal(User user)
		{
			return GetService<SignInManager>().CreateUserPrincipalAsync(user).Result;
		}
	}
}