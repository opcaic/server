using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace OPCAIC.ApiService.Security
{
	public class PermissionRequirement<TEnum> : IAuthorizationRequirement
		where TEnum : Enum
	{
		public PermissionRequirement(TEnum requiredPermission)
		{
			RequiredPermission = requiredPermission;
		}

		public TEnum RequiredPermission { get; }
	}
}