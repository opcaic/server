﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Security;
using OPCAIC.Infrastructure.Entities;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Security
{
	public class PermissionCoverageTest : AuthorizationTest
	{
		/// <inheritdoc />
		public PermissionCoverageTest(ITestOutputHelper output) : base(output)
		{
			ClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity
			(
				new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, "1"),
					new Claim(RolePolicy.UserRoleClaim, UserRole.User.ToString())
				}
			));
		}


		[Fact]
		public Task TournamentPermissions()
		{
			return DoCheckPermission<TournamentPermission, Tournament>();
		}

		[Fact]
		public Task UserPermissions()
		{
			return DoCheckPermission<UserPermission, User>();
		}

		[Fact]
		public Task GamePermission()
		{
			return DoCheckPermission<GamePermission, Game>();
		}

		[Fact]
		public Task EmailPermission()
		{
			return DoCheckPermission<EmailPermission, Email>();
		}

		[Fact]
		public Task DocumentPermission()
		{
			return DoCheckPermission<DocumentPermission, Document>();
		}

		[Fact]
		public Task SubmissionPermission()
		{
			return DoCheckPermission<SubmissionPermission, Submission>();
		}

		[Fact]
		public Task SubmissionValidationPermission()
		{
			return DoCheckPermission<SubmissionValidationPermission, SubmissionValidation>();
		}

		[Fact]
		public Task MatchPermission()
		{
			return DoCheckPermission<MatchPermission, Infrastructure.Entities.Match>();
		}

		[Fact]
		public Task MatchExecutionPermission()
		{
			return DoCheckPermission<MatchExecutionPermission, MatchExecution>();
		}

		private async Task DoCheckPermission<TPermission, TEntity>()
			where TPermission : Enum
			where TEntity : class, IEntity
		{
			var e = NewTrackedEntity<TEntity>();
			await DbContext.SaveChangesAsync();

			foreach (var permission in Enum.GetValues(typeof(TPermission)))
			{
				// no need to check return value, we only care about the permission being handled without exception
				await AuthorizationService.AuthorizeAsync(ClaimsPrincipal, (long?) e.Id,
					new PermissionRequirement<TPermission>((TPermission) permission));
			}
		}


		public static object[] GetEnumMembers(Type enumType)
		{
			return (object[]) Enum.GetValues(enumType);
		}
	}
}