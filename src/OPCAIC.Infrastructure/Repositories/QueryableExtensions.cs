﻿using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using OPCAIC.Infrastructure.Dtos.Games;

namespace OPCAIC.Infrastructure.Repositories
{
	public static class QueryableExtensions
	{
		#region Users

		public static IQueryable<User> Filter(this IQueryable<User> query, UserFilterDto filter)
		{
			if (filter.Email != null)
				query = query.Where(row => row.Email.ToUpper().StartsWith(filter.Email.ToUpper()));

			if (filter.Username != null)
				query = query.Where(row => row.Username.ToUpper().StartsWith(filter.Username.ToUpper()));

			if (filter.EmailVerified != null)
				query = query.Where(row => row.EmailVerified == filter.EmailVerified.Value);

			if (filter.UserRole != null)
				query = query.Where(row => row.RoleId == filter.UserRole.Value);

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
					return query.Sort(row => row.Username, asc);
				default:
					return query.Sort(row => row.Id, asc);
			}
		}

		#endregion

		#region Games

		public static IQueryable<Game> Filter(this IQueryable<Game> query, GameFilterDto filter)
		{
			if (filter.Name != null)
				query = query.Where(row => row.Name.ToUpper().StartsWith(filter.Name.ToUpper()));

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

		private static IQueryable<TEntity> Sort<TEntity, TKey>(this IQueryable<TEntity> query, Expression<Func<TEntity, TKey>> selector, bool asc)
		{
			if (asc)
				return query.OrderBy(selector);
			else
				return query.OrderByDescending(selector);
		}
	}
}
