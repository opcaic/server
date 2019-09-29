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
	public class MatchPermissionHandler
		: ResourcePermissionAuthorizationHandler<MatchPermission>
	{
		private readonly IRepository<Match> repository;

		public MatchPermissionHandler(IRepository<Match> repository)
		{
			this.repository = repository;
		}

		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(ClaimsPrincipal user,
			MatchPermission permission, long? id)
		{
			switch (permission)
			{
				case MatchPermission.Read:
					// authors of participated submissions and tournament managers
					var userId = user.TryGetId();
					return repository.GetStructAsync(id.Value, m =>
						!m.Tournament.PrivateMatchlog ||
						m.Participations.Any(s => s.Submission.AuthorId == userId) ||
						m.Tournament.OwnerId == userId ||
						m.Tournament.Managers.Any(mm => mm.UserId == userId));

				case MatchPermission.QueueMatchExecution:
					return Task.FromResult(false); // only admin //TODO: not tournament managers?

				case MatchPermission.Search:
					return Task.FromResult(true);

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}