using System;
using System.Security.Claims;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class GamePermissionHandler : ResourcePermissionAuthorizationHandler<GamePermission>
	{
		private readonly IGameRepository gameRepository;

		public GamePermissionHandler(IGameRepository gameRepository)
		{
			this.gameRepository = gameRepository;
		}

		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(long userId, ClaimsPrincipal user, GamePermission permission,
			long resourceId)
		{
			switch (permission)
			{
				case GamePermission.Create:
				case GamePermission.Update:
				case GamePermission.Delete:
					return Task.FromResult(false); // only admin, and he has his own handler

				case GamePermission.Read:
				case GamePermission.Search:
					return Task.FromResult(true); // TODO: Verify, or maybe more granular level (details or not)
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}