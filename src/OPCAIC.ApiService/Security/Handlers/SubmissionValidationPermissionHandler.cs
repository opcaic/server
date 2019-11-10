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
	public class SubmissionValidationPermissionHandler
		: ResourcePermissionAuthorizationHandler<SubmissionValidationPermission>
	{
		private readonly IRepository<SubmissionValidation> repository;

		public SubmissionValidationPermissionHandler(IRepository<SubmissionValidation> repository)
		{
			this.repository = repository;
		}

		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(ClaimsPrincipal user,
			SubmissionValidationPermission permission, long? id)
		{
			switch (permission)
			{
				case SubmissionValidationPermission.UploadResult:
					// worker only
					return Task.FromResult(user.HasClaim(WorkerClaimTypes.ValidationId,
						id.ToString()));

				case SubmissionValidationPermission.Search:
					return Task.FromResult(false);

				case SubmissionValidationPermission.DownloadResult:
				case SubmissionValidationPermission.ReadDetail:
				{
					var userId = user.TryGetId();
					// submission owners and tournament organizers
					return repository.GetStructAsync(id.Value, v =>
						v.Submission.AuthorId == userId ||
						v.Submission.Tournament.OwnerId == userId ||
						v.Submission.Tournament.Managers.Any(m => m.UserId == userId));
				}
				case SubmissionValidationPermission.ReadAdmin:
				{
					var userId = user.TryGetId();
					// tournament organizers
					return repository.GetStructAsync(id.Value, v =>
						v.Submission.Tournament.OwnerId == userId ||
						v.Submission.Tournament.Managers.Any(m => m.UserId == userId));
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}