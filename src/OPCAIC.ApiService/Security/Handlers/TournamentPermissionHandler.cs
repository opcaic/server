using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class TournamentPermissionHandler
		: ResourcePermissionAuthorizationHandler<TournamentPermission>
	{
		private readonly IRepository<Tournament> repository;

		public TournamentPermissionHandler(IRepository<Tournament> repository)
		{
			this.repository = repository;
		}

		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(ClaimsPrincipal user,
			TournamentPermission permission,
			long? id)
		{
			var userId = user.TryGetId();

			switch (permission)
			{
				case TournamentPermission.Create:
					// only organizers
					return Task.FromResult(user.GetUserRole() == UserRole.Organizer);

				case TournamentPermission.ManageAdditionalFiles:
				case TournamentPermission.StartEvaluation:
				case TournamentPermission.ManageInvites:
				case TournamentPermission.Update:
				case TournamentPermission.StopEvaluation:
				case TournamentPermission.PauseEvaluation:
				case TournamentPermission.UnpauseEvaluation:
				case TournamentPermission.Publish:
				case TournamentPermission.ManageDocuments:
				case TournamentPermission.ReadManagers:
				case TournamentPermission.ReadAdmin:
					// only owner and managers
					return IsOwnerOrManager(userId, id);

				case TournamentPermission.Delete:
				case TournamentPermission.ManageManagers:
					// only owner
					return IsOwner(userId, id);

				case TournamentPermission.Read:
				case TournamentPermission.Search:
					// everyone
					return Task.FromResult(true);

				case TournamentPermission.Submit:
				case TournamentPermission.Join:
				case TournamentPermission.ViewLeaderboard:
					return IsVisible(userId, id);

				case TournamentPermission.DownloadAdditionalFiles:
					if (user.HasClaim(WorkerClaimTypes.TournamentId, id.ToString()))
					{
						return Task.FromResult(true);
					}

					return IsOwnerOrManager(userId, id);
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}

		private Task<bool> IsOwner(long userId, long? tournamentId)
		{
			return repository.GetStructAsync(tournamentId.Value, t =>
				t.OwnerId == userId);
		}

		private Task<bool> IsOwnerOrManager(long userId, long? tournamentId)
		{
			return repository.GetStructAsync(tournamentId.Value, t =>
				t.OwnerId == userId ||
				t.Managers.Any(m => m.UserId == userId));
		}

		public class VisibilityData
		{
			public long OwnerId { get; set; }
			public TournamentAvailability Availability { get; set; }
			public long[] ManagerIds { get; set; }
			public long[] ParticipantIds { get; set; }
		}

		private async Task<bool> IsVisible(long userId, long? tournamentId)
		{
			var data = await repository.GetAsync(tournamentId.Value,
				t => new VisibilityData
				{
					OwnerId = t.OwnerId, 
					Availability = t.Availability, 
					ManagerIds = t.Managers.Select(m => m.UserId).ToArray(),
					ParticipantIds = t.Participants.Select(m => m.UserId).ToArray()
				});

			switch (data.Availability)
			{
				case TournamentAvailability.Public:
					// everyone
					return true;

				case TournamentAvailability.Private:
					// only invited people and managers
					return data.ParticipantIds.Contains(userId) ||
						data.OwnerId == userId ||
						data.ManagerIds.Contains(userId);

				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}