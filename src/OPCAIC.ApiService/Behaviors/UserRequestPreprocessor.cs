using System.Threading;
using System.Threading.Tasks;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Http;
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
			var user = httpContextAccessor.HttpContext?.User;
			if (user == null || !user.Identity.IsAuthenticated)
			{
				return Task.CompletedTask;
			}

			switch (request)
			{
				case IPublicRequest userRequest:
				{
					if (user.TryGetId(out var id))
					{
						userRequest.RequestingUserId = id;
					}

					userRequest.RequestingUserRole = user.GetUserRole();

					break;
				}

				case IAuthenticatedRequest authRequest:
				{
					if (user.TryGetId(out var id))
					{
						authRequest.RequestingUserId = id;
					}

					authRequest.RequestingUserRole = user.GetUserRole();

					break;
				}
			}

			return Task.CompletedTask;
		}
	}
}