using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Application.Infrastructure;

namespace OPCAIC.ApiService.Behaviors
{
	public class UserRequestPreprocessor<TRequest> : IRequestPreProcessor<TRequest>
	{
		private readonly IHttpContextAccessor httpContextAccessor;

		public UserRequestPreprocessor(IHttpContextAccessor httpContextAccessor)
		{
			this.httpContextAccessor = httpContextAccessor;
		}

		/// <inheritdoc />
		public Task Process(TRequest request, CancellationToken cancellationToken)
		{
			var user = httpContextAccessor.HttpContext.User;
			if (request is IPublicRequest userRequest)
			{
				if (user.Identity.IsAuthenticated)
				{
					if (user.TryGetId(out var id))
					{
						userRequest.RequestingUserId = id;
					}

					userRequest.RequestingUserRole = user.GetUserRole();
				}
			}
			else if (request is IAuthenticatedRequest authRequest)
			{
				if (user.Identity.IsAuthenticated)
				{
					if (user.TryGetId(out var id))
					{
						authRequest.RequestingUserId = id;
					}

					authRequest.RequestingUserRole = user.GetUserRole();
				}
			}

			return Task.CompletedTask;
		}
	}
}