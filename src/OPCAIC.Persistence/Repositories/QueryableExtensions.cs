using System;
using System.Linq;
using System.Linq.Expressions;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Persistence.Repositories
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

		#region TournamentInvitations

		public static IQueryable<TournamentInvitation> Filter(
			this IQueryable<TournamentInvitation> query, TournamentInvitationFilterDto filter)
		{
			return query.SortBy(filter.SortBy, filter.Asc);
		}

		private static IQueryable<TournamentInvitation> SortBy(
			this IQueryable<TournamentInvitation> query, string sortBy, bool asc)
		{
			switch (sortBy)
			{
				case TournamentInvitationFilterDto.SortByEmail:
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
				query = query.Where(row => row.TournamentParticipation.ActiveSubmissionId == row.Id);
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

	}
}