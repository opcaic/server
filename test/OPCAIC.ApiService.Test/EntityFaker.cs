﻿using System;
using System.Collections.Generic;
using Bogus;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using static OPCAIC.ApiService.Test.Services.TestMapper;

namespace OPCAIC.ApiService.Test
{
	public class EntityFaker
	{
		private readonly Dictionary<Type, object> fakers = new Dictionary<Type, object>();
		private readonly int seed;

		public EntityFaker(int seed = 10)
		{
			this.seed = seed;
			Configure<User>()
				.RuleFor(u => u.FirstName, f => f.Name.FirstName())
				.RuleFor(u => u.LastName, f => f.Name.LastName())
				.RuleFor(u => u.UserName, f => f.Name.Random.String(8))
				.RuleFor(u => u.Email, f => f.Internet.Email())
				.RuleFor(u => u.LocalizationLanguage, f => f.PickRandomParam("cz", "en"))
				.RuleFor(u => u.Organization, "OPCAIC");

			Configure<Game>()
				.RuleFor(g => g.Name, f => f.Random.String(10))
				.RuleFor(g => g.Description, f => f.Lorem.Paragraph())
				.RuleFor(g => g.GameType, f => f.PickRandom<GameType>())
				.RuleFor(g => g.MaxAdditionalFilesSize, 1024 * 1024)
				.RuleFor(g => g.ConfigurationSchema, "{}");

			Configure<Tournament>()
				.RuleFor(g => g.Name, f => f.Random.String(10))
				.RuleFor(g => g.Description, f => f.Lorem.Paragraph())
				.RuleFor(g => g.Availability, TournamentAvailability.Public)
				.RuleFor(t => t.Deadline, DateTime.Now + TimeSpan.FromHours(1))
				.RuleFor(g => g.MaxSubmissionSize, 1024 * 1024)
				.RuleFor(g => g.Matches, f => new List<Match>());

			Configure<Submission>()
				.RuleFor(s => s.Author, f => Entity<User>());

			Configure<SubmissionValidation>()
				.RuleFor(v => v.Submission, i => Entity<Submission>());

			Configure<SubmissionParticipation>()
				.RuleFor(p => p.Submission, f => Entity<Submission>())
				.RuleFor(p => p.Order, f => f.IndexVariable++);

			Configure<Document>()
				.RuleFor(d => d.Content, f => f.Lorem.Paragraph())
				.RuleFor(d => d.Tournament, f => Entity<Tournament>());

			Configure<Match>()
				.RuleFor(m => m.Executions, f => new List<MatchExecution>
				{
					new MatchExecution
					{
						ExecutorResult = EntryPointResult.Success
					}
				});
		}

		public Faker<T> Configure<T>() where T : class
		{
			if (!fakers.TryGetValue(typeof(T), out var obj))
			{
				obj = fakers[typeof(T)] = new Faker<T>().UseSeed(seed);
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
			var dto = Mapper.Map<TDto>(Configure<TEntity>().Generate());
			additionalSetup(dto);
			return dto;
		}

		public TDto Dto<TEntity, TDto>() where TEntity : class
		{
			return Dto<TEntity, TDto>(d => { });
		}
	}

}