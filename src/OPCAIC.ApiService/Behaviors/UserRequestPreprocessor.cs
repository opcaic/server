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
			if (request is IUserRequest userRequest)
			{
				var user = httpContextAccessor.HttpContext.User;
				if (user.Identity.IsAuthenticated && user.TryGetId(out var id))
				{
					userRequest.RequestingUserId = id;
				}
			}

			return Task.CompletedTask;
		}
	}
}