using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Interfaces.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class GamePermissionHandler
		: ResourcePermissionAuthorizationHandler<GamePermission, EmptyAuthData>
	{
		private readonly IGameRepository gameRepository;

		public GamePermissionHandler(IGameRepository gameRepository)
		{
			this.gameRepository = gameRepository;
		}

		/// <inheritdoc />
		protected override Task<EmptyAuthData> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return Task.FromResult(EmptyAuthData.Instance);
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(ClaimsPrincipal user,
			GamePermission permission,
			EmptyAuthData authData)
		{
			switch (permission)
			{
				case GamePermission.Create:
				case GamePermission.Update:
				case GamePermission.Delete:
					return false; // only admin, and he has his own handler

				case GamePermission.Read:
				case GamePermission.Search:
					return true; // TODO: Verify, or maybe more granular level (details or not)
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}