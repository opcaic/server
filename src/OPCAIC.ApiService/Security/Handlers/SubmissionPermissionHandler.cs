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
	public class SubmissionPermissionHandler
		: ResourcePermissionAuthorizationHandler<SubmissionPermission>
	{
		private readonly IRepository<Submission> repository;

		public SubmissionPermissionHandler(IRepository<Submission> repository)
		{
			this.repository = repository;
		}

		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(ClaimsPrincipal user,
			SubmissionPermission permission, long? id)
		{
			var userId = user.TryGetId();

			switch (permission)
			{
				case SubmissionPermission.Read:
				case SubmissionPermission.Download:
					// Authors tournament managers and workers
					if (user.HasClaim(WorkerClaimTypes.SubmissionId, id.ToString()))
					{
						return Task.FromResult(true);
					}

					return repository.GetStructAsync(id.Value, s =>
						s.AuthorId == userId ||
						s.Tournament.OwnerId == userId ||
						s.Tournament.Managers.Any(m => m.UserId == userId));

				case SubmissionPermission.Update:
					// authors only
					return repository.GetStructAsync(id.Value, s =>
						s.AuthorId == userId);

				case SubmissionPermission.Search:
					return Task.FromResult(true);

				case SubmissionPermission.QueueValidation:
					return Task.FromResult(false); // admin only

				case SubmissionPermission.ReadAdmin:
					return repository.GetStructAsync(id.Value, s =>
						s.Tournament.OwnerId == userId ||
						s.Tournament.Managers.Any(m => m.UserId == userId));
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}