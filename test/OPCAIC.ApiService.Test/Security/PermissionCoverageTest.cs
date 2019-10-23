using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Moq;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Security.Handlers;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Xunit;
using Xunit.Abstractions;
using Match = OPCAIC.Domain.Entities.Match;

namespace OPCAIC.ApiService.Test.Security
{
	public class PermissionCoverageTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public PermissionCoverageTest(ITestOutputHelper output) : base(output)
		{
		}

		private async Task CheckPermissions<TPermission, TEntity, THandler>()
			where TPermission : Enum
			where TEntity : class, IEntity
			where THandler : AuthorizationHandler<PermissionRequirement<TPermission>, ResourceId>
		{
			var mock = Services.Mock<IRepository<TEntity>>();
			mock.Setup(r
					=> r.ListAsync(It.IsAny<IProjectingSpecification<TEntity, bool>>(), default))
				.ReturnsAsync(new List<bool> {true});

			await DoCheckPermissions<TPermission, TEntity, THandler>();
		}

		private async Task DoCheckPermissions<TPermission, TEntity, THandler>()
			where TPermission : Enum
			where TEntity : class, IEntity
			where THandler : AuthorizationHandler<PermissionRequirement<TPermission>, ResourceId>
		{
			var user = new ClaimsPrincipal(new ClaimsIdentity
			(
				new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, "1"),
					new Claim(RolePolicy.UserRoleClaim, UserRole.User.ToString())
				}
			));

			var handler = GetService<THandler>();

			foreach (var permission in Enum.GetValues(typeof(TPermission)))
			{
				// no need to check return value, we only care about the permission being handled without exception
				await handler.HandleAsync(new AuthorizationHandlerContext(
					new[] {new PermissionRequirement<TPermission>((TPermission)permission)}, user,
					new ResourceId(1)));
			}
		}

		[Fact]
		public Task DocumentPermission()
		{
			return CheckPermissions<DocumentPermission, Document, DocumentPermissionHandler>(); 
		}

		[Fact(Skip = "Unused right now")]
		public Task EmailPermission()
		{
//			return CheckPermissions<EmailPermission, Email>();
			return Task.CompletedTask;
		}

		[Fact]
		public Task GamePermission()
		{
			return CheckPermissions<GamePermission, Game, GamePermissionHandler>();
		}

		[Fact]
		public Task MatchExecutionPermission()
		{
			return CheckPermissions<MatchExecutionPermission, MatchExecution,
				MatchExecutionPermissionHandler>();
		}

		[Fact]
		public Task MatchPermission()
		{
			return CheckPermissions<MatchPermission, Match, MatchPermissionHandler>();
		}

		[Fact]
		public Task SubmissionPermission()
		{
			return
				CheckPermissions<SubmissionPermission, Submission, SubmissionPermissionHandler>();
		}

		[Fact]
		public Task SubmissionValidationPermission()
		{
			return CheckPermissions<SubmissionValidationPermission, SubmissionValidation,
				SubmissionValidationPermissionHandler>();
		}


		[Fact]
		public Task TournamentPermissions()
		{
			// make sure valid data are used...
			var mock = Services.Mock<IRepository<Tournament>>();
			mock.Setup(r
					=> r.ListAsync(It.IsAny<IProjectingSpecification<Tournament, bool>>(), default))
				.ReturnsAsync(new List<bool> {true});
			mock.Setup(r
				=> r.FindAsync(
					It.IsAny<IProjectingSpecification<Tournament,
						TournamentPermissionHandler.VisibilityData>>(), default)).ReturnsAsync(
				new TournamentPermissionHandler.VisibilityData
				{
					Availability = TournamentAvailability.Public
				});

			return
				DoCheckPermissions<TournamentPermission, Tournament, TournamentPermissionHandler>();
		}

		[Fact]
		public Task UserPermissions()
		{
			return CheckPermissions<UserPermission, User, UserPermissionHandler>();
		}
	}
}