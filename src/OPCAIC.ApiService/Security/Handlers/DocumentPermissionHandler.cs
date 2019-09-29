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
	public class DocumentPermissionHandler
		: ResourcePermissionAuthorizationHandler<DocumentPermission>
	{
		private readonly IRepository<Document> repository;

		public DocumentPermissionHandler(IRepository<Document> repository)
		{
			this.repository = repository;
		}

		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(ClaimsPrincipal user,
			DocumentPermission permission,
			long? id)
		{
			switch (permission)
			{
				case DocumentPermission.Search:
				case DocumentPermission.Read:
					return Task.FromResult(true); // TODO: private tournaments

				case DocumentPermission.Create:
				case DocumentPermission.Update:
				case DocumentPermission.Delete:
					// only tournament owners and managers
					var userId = user.TryGetId();
					return repository.GetStructAsync(id.Value, d =>
						d.Tournament.OwnerId == userId ||
						d.Tournament.Managers.Any(m => m.UserId == userId));

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}