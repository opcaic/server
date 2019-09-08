﻿using System;
using System.Collections.Generic;
using AutoMapper;
using Bogus;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Test
{
	public class EntityFaker
	{
		private readonly Dictionary<Type, object> fakers = new Dictionary<Type, object>();
		private static readonly IMapper mapper = new Mapper(MapperConfigurationFactory.Create());

		public EntityFaker(int seed = 10)
		{
			AddFaker(new Faker<User>()
				.UseSeed(seed)
				.RuleFor(u => u.FirstName, f => f.Name.FirstName())
				.RuleFor(u => u.LastName, f => f.Name.LastName())
				.RuleFor(u => u.UserName, f => f.Name.Random.String(8))
				.RuleFor(u => u.Email, f => f.Internet.Email())
				.RuleFor(u => u.LocalizationLanguage, f => f.PickRandomParam("cz", "en"))
				.RuleFor(u => u.Organization, "OPCAIC"));

			AddFaker(new Faker<Game>()
				.UseSeed(seed)
				.RuleFor(g => g.Name, f => f.Random.String(10))
				.RuleFor(g => g.Description, f => f.Lorem.Paragraph())
				.RuleFor(g => g.GameType, f => f.PickRandom<GameType>())
				.RuleFor(g => g.ConfigurationSchema, "{}"));

			AddFaker(new Faker<Tournament>()
				.UseSeed(seed)
				.RuleFor(g => g.Name, f => f.Random.String(10))
				.RuleFor(g => g.Description, f => f.Lorem.Paragraph())
				.RuleFor(g => g.Availability, TournamentAvailability.Public)
				.RuleFor(t => t.Deadline, DateTime.Now + TimeSpan.FromHours(1)));

			AddFaker(new Faker<Submission>()
				.UseSeed(seed)
				.RuleFor(s => s.Author, f => Entity<User>())
				.RuleFor(s => s.IsActive, true));

			AddFaker(new Faker<SubmissionParticipation>()
				.UseSeed(seed)
				.RuleFor(p => p.Submission, f => Entity<Submission>())
				.RuleFor(p => p.Order, f => f.IndexVariable++));

			AddFaker(new Faker<Match>()
				.UseSeed(seed)
				.RuleFor(m => m.Executions, f => new List<MatchExecution>
				{
					new MatchExecution
					{
						ExecutorResult = EntryPointResult.Success
					}
				}));
		}

		private void AddFaker<T>(Faker<T> faker) where T : class
		{
			fakers.Add(typeof(T), faker);
		}

		public Faker<T> Configure<T>() where T : class
		{
			if (!fakers.TryGetValue(typeof(T), out var obj))
			{
				obj = fakers[typeof(T)] = new Faker<T>();
			}

			return (Faker<T>) obj;
		}

		public T Entity<T>() where T : class
		{
			return Configure<T>().Generate();
		}

		public List<T> Entities<T>(int count) where T : class
		{
			return Configure<T>().Generate(count);
		}

		public TDto Dto<TEntity, TDto>(Action<TDto> additionalSetup) where TEntity : class
		{
			var dto = mapper.Map<TDto>(Configure<TEntity>().Generate());
			additionalSetup(dto);
			return dto;
		}

		public TDto Dto<TEntity, TDto>() where TEntity : class
		{
			return Dto<TEntity, TDto>(d => { });
		}
	}

}