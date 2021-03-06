﻿using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Infrastructure
{
	/// <summary>
	///     Exposes id of the user who is making the request.
	/// </summary>
	public interface IAuthenticatedRequest
	{
		/// <summary>
		///     Id of the user making the request.
		/// </summary>
		long RequestingUserId { get; set; }

		/// <summary>
		///     Role of the user making the request.
		/// </summary>
		UserRole RequestingUserRole { get; set; }
	}
}