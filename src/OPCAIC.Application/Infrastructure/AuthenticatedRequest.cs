using AutoMapper;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Infrastructure
{
	public abstract class AuthenticatedRequest : IAuthenticatedRequest
	{
		/// <inheritdoc />
		[IgnoreMap]
		public long RequestingUserId { get; set; }

		/// <inheritdoc />
		[IgnoreMap]
		public UserRole RequestingUserRole { get; set; }
	}
}