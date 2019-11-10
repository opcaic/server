using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class MatchExecutionPermissionHandler
		: ResourcePermissionAuthorizationHandler<MatchExecutionPermission>
	{
		private readonly IRepository<MatchExecution> repository;

		public MatchExecutionPermissionHandler(IRepository<MatchExecution> repository)
		{
			this.repository = repository;
		}

		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(ClaimsPrincipal user,
			MatchExecutionPermission permission, long? id)
		{
			switch (permission)
			{
				case MatchExecutionPermission.UploadResult:
					return Task.FromResult(user.HasClaim(WorkerClaimTypes.ExecutionId,
						id.ToString()));

				case MatchExecutionPermission.Search:
					return Task.FromResult(false);

				case MatchExecutionPermission.Read:
				case MatchExecutionPermission.DownloadResults:
				{
					var userId = user.TryGetId();

					return repository.GetStructAsync(id.Value, e =>
						e.Match.Tournament.OwnerId == userId ||
						e.Match.Tournament.Managers.Any(m => m.UserId == userId) ||
						e.Match.Participations.Any(s => s.Submission.AuthorId == userId));
				}
				case MatchExecutionPermission.ReadAdmin:
				{
					var userId = user.TryGetId();
					return repository.GetStructAsync(id.Value, e =>
						e.Match.Tournament.OwnerId == userId ||
						e.Match.Tournament.Managers.Any(m => m.UserId == userId));
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}