using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace OPCAIC.ApiService.Security
{
	public class PermissionRequirement<TEnum> : IAuthorizationRequirement
		where TEnum : Enum
	{
		public PermissionRequirement(params TEnum[] requiredPermissions)
		{
			RequiredPermissions = requiredPermissions;
		}

		public IEnumerable<TEnum> RequiredPermissions { get; }
	}

	public class TournamentManagerRequirement : IAuthorizationRequirement
	{
	}

	public class OwnerRequirement : IAuthorizationRequirement
	{
	}

	public class TournamentAccessRequirement : IAuthorizationRequirement
	{
	}
}