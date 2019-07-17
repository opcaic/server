using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Security;
using System;
using System.Linq;
using System.Security.Principal;

namespace OPCAIC.ApiService.Controllers
{
	public class ApiController: ControllerBase
	{
		protected void CheckTournamentAccess(long tournamentId, IIdentity idenity)
		{
			var user = idenity as UserIdentity;
			if (user == null)
				throw new UnauthorizedExcepion($"User is not authorized");

			if (!user.ManagedTournamentIds.Contains(tournamentId))
				throw new ForbiddenException($"User does not have permission to tournament with id '{tournamentId}'");
		}
	}
}
