using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Security;
using OPCAIC.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Security
{
	public class TournamentAuthorizationTest : AuthorizationTest
	{
		/// <inheritdoc />
		public TournamentAuthorizationTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public void AllowTournamentManagersUpdate()
		{
			var game = NewTrackedEntity<Game>();
			var manager = NewUser();
			var owner = NewUser();
			var tournament = NewTrackedEntity<Tournament>();

			tournament.Game = game;
			tournament.Owner = owner;
			tournament.Managers = new List<TournamentManager>
			{
				new TournamentManager
				{
					User = manager,
					Tournament = tournament
				}
			};

			DbContext.SaveChanges();

			AuthorizationService.CheckPermissions(GetClaimsPrincipal(manager), tournament.Id,
				TournamentPermission.Update);
		}
	}
}