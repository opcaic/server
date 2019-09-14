using System;
using System.Linq;
using System.Linq.Expressions;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Repositories
{
	public static class QueryableExtensions
	{
		private static IQueryable<TEntity> Sort<TEntity, TKey>(this IQueryable<TEntity> query,
			Expression<Func<TEntity, TKey>> selector, bool asc)
		{
			if (asc)
			{
				return query.OrderBy(selector);
			}

			return query.OrderByDescending(selector);
		}

		#region Users

		public static IQueryable<User> Filter(this IQueryable<User> query, UserFilterDto filter)
		{
			if (filter.Email != null)
			{
				query = query.Where(row => row.Email.ToUpper().StartsWith(filter.Email.ToUpper()));
			}

			if (filter.Username != null)
			{
				query = query.Where(row
					=> row.UserName.ToUpper().StartsWith(filter.Username.ToUpper()));
			}

			if (filter.EmailVerified != null)
			{
				query = query.Where(row => row.EmailConfirmed == filter.EmailVerified.Value);
			}

			if (filter.UserRole != null)
			{
				query = query.Where(row => row.RoleId == filter.UserRole.Value);
			}

			return query.SortBy(filter.SortBy, filter.Asc);
		}

		private static IQueryable<User> SortBy(this IQueryable<User> query, string sortBy, bool asc)
		{
			switch (sortBy)
			{
				case UserFilterDto.SortByCreated:
					return query.Sort(row => row.Created, asc);
				case UserFilterDto.SortByEmail:
					return query.Sort(row => row.Email, asc);
				case UserFilterDto.SortByUsername:
					return query.Sort(row => row.UserName, asc);
				default:
					return query.Sort(row => row.Id, asc);
			}
		}

		#endregion

		#region Games

		public static IQueryable<Game> Filter(this IQueryable<Game> query, GameFilterDto filter)
		{
			if (filter.Name != null)
			{
				query = query.Where(row => row.Name.ToUpper().StartsWith(filter.Name.ToUpper()));
			}

			return query.SortBy(filter.SortBy, filter.Asc);
		}

		private static IQueryable<Game> SortBy(this IQueryable<Game> query, string sortBy, bool asc)
		{
			switch (sortBy)
			{
				case GameFilterDto.SortByCreated:
					return query.Sort(row => row.Created, asc);
				case GameFilterDto.SortByName:
					return query.Sort(row => row.Name, asc);
				default:
					return query.Sort(row => row.Id, asc);
			}
		}

		#endregion

		#region Tournaments

		public static IQueryable<Tournament> Filter(this IQueryable<Tournament> query,
			TournamentFilterDto filter)
		{
			if (filter.Name != null)
			{
				query = query.Where(row => row.Name.ToUpper().StartsWith(filter.Name.ToUpper()));
			}

			// TODO(ON): check how sql queries are generated and whether it is optimal or not (comparing nullable types instead of using .Value)
			if (filter.GameId != null)
			{
				query = query.Where(row => row.GameId == filter.GameId);
			}

			if (filter.Format != null)
			{
				query = query.Where(row => row.Format == filter.Format);
			}

			if (filter.Scope != null)
			{
				query = query.Where(row => row.Scope == filter.Scope);
			}

			if (filter.RankingStrategy != null)
			{
				query = query.Where(row => row.RankingStrategy == filter.RankingStrategy);
			}

			return query.SortBy(filter.SortBy, filter.Asc);
		}

		private static IQueryable<Tournament> SortBy(this IQueryable<Tournament> query,
			string sortBy, bool asc)
		{
			switch (sortBy)
			{
				case TournamentFilterDto.SortByCreated:
					return query.Sort(row => row.Created, asc);
				case TournamentFilterDto.SortByName:
					return query.Sort(row => row.Name, asc);
				default:
					return query.Sort(row => row.Id, asc);
			}
		}

		#endregion

		#region TournamentParticipants

		public static IQueryable<TournamentParticipant> Filter(
			this IQueryable<TournamentParticipant> query, TournamentParticipantFilterDto filter)
		{
			return query.SortBy(filter.SortBy, filter.Asc);
		}

		private static IQueryable<TournamentParticipant> SortBy(
			this IQueryable<TournamentParticipant> query, string sortBy, bool asc)
		{
			switch (sortBy)
			{
				case TournamentParticipantFilterDto.SortByEmail:
					return query.Sort(row => row.Email, asc);
				default:
					return query.Sort(row => row.Id, asc);
			}
		}

		#endregion

		#region Documents

		public static IQueryable<Document> Filter(this IQueryable<Document> query,
			DocumentFilterDto filter)
		{
			if (filter.Name != null)
			{
				query = query.Where(row => row.Name.ToUpper().StartsWith(filter.Name.ToUpper()));
			}

			if (filter.TournamentId != null)
			{
				query = query.Where(row => row.TournamentId == filter.TournamentId);
			}

			return query.SortBy(filter.SortBy, filter.Asc);
		}

		private static IQueryable<Document> SortBy(this IQueryable<Document> query, string sortBy,
			bool asc)
		{
			switch (sortBy)
			{
				case DocumentFilterDto.SortByCreated:
					return query.Sort(row => row.Created, asc);
				case DocumentFilterDto.SortByName:
					return query.Sort(row => row.Name, asc);
				default:
					return query.Sort(row => row.Id, asc);
			}
		}

		#endregion

		#region Submissions

		public static IQueryable<Submission> Filter(this IQueryable<Submission> query,
			SubmissionFilterDto filter)
		{
			if (filter.AuthorId != null)
			{
				query = query.Where(row => row.AuthorId == filter.AuthorId);
			}

			if (filter.Author != null)
			{
				query = query.Where(row => row.Author.UserName.Contains(filter.Author));
			}

			if (filter.IsActive != null)
			{
				query = query.Where(row => row.IsActive == filter.IsActive);
			}

			if (filter.TournamentId != null)
			{
				query = query.Where(row => row.TournamentId == filter.TournamentId);
			}

			if (filter.MatchId != null)
			{
				query = query.Where(row
					=> row.Participations.Any(m => m.MatchId == filter.MatchId));
			}

			if (filter.ValidationState != null)
			{
				// there should always be at least one validation
				switch (filter.ValidationState.Value)
				{
					case SubmissionValidationState.Queued:
						query = query.Where(row
							=> row.Validations.Last().State <= WorkerJobState.Waiting);
						break;
					case SubmissionValidationState.Valid:
						query = query.Where(row
							=> row.Validations.Last().ValidatorResult == EntryPointResult.Success);
						break;
					case SubmissionValidationState.Invalid:
						query = query.Where(row
							=> row.Validations.Last().ValidatorResult == EntryPointResult.UserError);
						break;
					case SubmissionValidationState.Error:
						query = query.Where(row
							=> row.Validations.Last().ValidatorResult >= EntryPointResult.ModuleError);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return query.SortBy(filter.SortBy, filter.Asc);
		}

		private static IQueryable<Submission> SortBy(this IQueryable<Submission> query,
			string sortBy,
			bool asc)
		{
			switch (sortBy)
			{
				case SubmissionFilterDto.SortByAuthor:
					return query.Sort(row => row.Author.UserName, asc);
				case SubmissionFilterDto.SortByCreated:
					return query.Sort(row => row.Created, asc);
				default:
					return query.Sort(row => row.Id, asc);
			}
		}

		#endregion

		#region Matches

		public static IQueryable<Match> Filter(this IQueryable<Match> query,
			MatchFilterDto filter)
		{
			if (filter.TournamentId != null)
			{
				query = query.Where(row => row.Tournament.Id == filter.TournamentId);
			}

			if (filter.UserId != null)
			{
				query = query.Where(row
					=> row.Participations.Any(p => p.Submission.AuthorId == filter.UserId));
			}

			if (filter.SubmissionId != null)
			{
				query = query.Where(row =>
					row.Participations.Any(p => p.SubmissionId == filter.SubmissionId));
			}

			switch (filter.State)
			{
				// a match should always have at least one execution
				case null:
					break; // nothing
				case MatchState.Queued:
					query = query.Where(row => !row.Executions
						.OrderByDescending(e => e.Created).First().Executed.HasValue);
					break;
				case MatchState.Executed:
					query = query.Where(row => row.Executions
							.OrderByDescending(e => e.Created).First().ExecutorResult ==
						EntryPointResult.Success);
					break;
				case MatchState.Failed:
					query = query.Where(row => row.Executions
							.OrderByDescending(e => e.Created).First().ExecutorResult >=
						EntryPointResult.UserError);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (filter.Username != null)
			{
				query = query.Where(row =>
					row.Participations.Any(s
						=> s.Submission.Author.UserName.Contains(filter.Username)));
			}

			return query.SortBy(filter.SortBy, filter.Asc);
		}

		private static IQueryable<Match> SortBy(this IQueryable<Match> query, string sortBy,
			bool asc)
		{
			switch (sortBy)
			{
				case MatchFilterDto.SortByCreated:
					return query.Sort(row => row.Created, asc);
				case MatchFilterDto.SortByUpdated:
					return query.Sort(row => row.Updated, asc);
				default:
					return query.Sort(row => row.Id, asc);
			}
		}

		#endregion
	}
}