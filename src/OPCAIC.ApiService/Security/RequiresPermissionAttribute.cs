using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OPCAIC.ApiService.Security
{
	public class RequiresPermissionAttribute : TypeFilterAttribute
	{
		public RequiresPermissionAttribute(params object[] permissions)
			: base(typeof(RequiresPermissionAttributeImpl))
		{
			Debug.Assert(permissions.All(p => p is Enum));
			var lookup = permissions.ToLookup(p => p.GetType());

			Arguments = new object[]
			{
				from @group in lookup
					let reqType = typeof(PermissionRequirement<>).MakeGenericType(@group.Key)
					select (IAuthorizationRequirement)Activator.CreateInstance(reqType, @group.ToArray())
			};
		}

		private class RequiresPermissionAttributeImpl : Attribute, IAsyncResourceFilter
		{
			private readonly IAuthorizationService authorizationService;
			private readonly IEnumerable<IAuthorizationRequirement> requirements;

			public RequiresPermissionAttributeImpl(IAuthorizationService authorizationService,
				IEnumerable<IAuthorizationRequirement> requirements)
			{
				this.authorizationService = authorizationService;
				this.requirements = requirements;
			}

			public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
				ResourceExecutionDelegate next)
			{
				if (context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) == null)
				{
					context.Result = new ChallengeResult();
					return;
				}

				if (!(await authorizationService.AuthorizeAsync(context.HttpContext.User, 0L, requirements)).Succeeded)
				{
					context.Result = new ForbidResult();
					return;
				}

				await next();
			}
		}
	}
}