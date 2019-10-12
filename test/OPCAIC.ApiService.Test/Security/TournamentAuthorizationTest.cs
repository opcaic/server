using System;
using System.Collections.Generic;
using System.Security.Claims;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Security;
using OPCAIC.Common;
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
			Services.AddMapper();
			Services.AddRepositories();
			UseDatabase();
		}

		[Fact]
		public void AllowEveryoneBrowseTournaments()
		{
			var game = NewTrackedEntity<Game>();
			var manager = NewUser();
			var owner = NewUser();
			var tournament = NewTrackedEntity<Tournament>();

			tournament.Game = game;
			tournament.Owner = owner;

			DbContext.SaveChanges();

			AuthorizationService.CheckPermissions(new ClaimsPrincipal(),
				TournamentPermission.Update);
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
				new TournamentManager {User = manager, Tournament = tournament}
			};

			DbContext.SaveChanges();

			AuthorizationService.CheckPermissions(GetClaimsPrincipal(manager), tournament.Id,
				TournamentPermission.Update);
		}
	}
}