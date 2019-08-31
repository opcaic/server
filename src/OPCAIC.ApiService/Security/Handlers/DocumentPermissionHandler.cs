using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class DocumentPermissionHandler
		: ResourcePermissionAuthorizationHandler<DocumentPermission, DocumentAuthDto>
	{
		private readonly IDocumentRepository documentRepository;

		public DocumentPermissionHandler(IDocumentRepository documentRepository)
		{
			this.documentRepository = documentRepository;
		}

		/// <inheritdoc />
		protected override Task<DocumentAuthDto> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return documentRepository.GetAuthorizationData(resourceId, cancellationToken);
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(ClaimsPrincipal user,
			DocumentPermission permission,
			DocumentAuthDto authData)
		{
			switch (permission)
			{
				case DocumentPermission.Search:
				case DocumentPermission.Read:
					return true; // TODO: private tournaments

				case DocumentPermission.Create:
				case DocumentPermission.Update:
				case DocumentPermission.Delete:
					// only tournament owners and managers
					var userId = user.TryGetId();
					return userId == authData.TournamentOwnerId ||
						authData.TournamentManagersIds.Contains(userId);

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}