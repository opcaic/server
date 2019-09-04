using System;
using System.Collections.Generic;
using AutoMapper;
using Bogus;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Test
{
	public class EntityFaker
	{
		private static readonly Dictionary<Type, object> fakers = new Dictionary<Type, object>();

		static EntityFaker()
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
				.RuleFor(g => g.GameType, f => f.PickRandom<GameType>())
				.RuleFor(g => g.ConfigurationSchema, (f, g) => g.ConfigurationSchema = "{}"));

			AddFaker(new Faker<Tournament>()
				.UseSeed(10)
				.RuleFor(g => g.Name, f => f.Random.String(10))
				.RuleFor(g => g.Description, f => f.Random.String())
				.RuleFor(g => g.Availability,
					(f, g) => g.Availability = TournamentAvailability.Public));
		}

		private static void AddFaker<T>(Faker<T> faker) where T : class
		{
			fakers.Add(typeof(T), faker);
		}

		public static T NewEntity<T>() where T : class
		{
			return ((Faker<T>) fakers[typeof(T)]).Generate();
		}
	}

}