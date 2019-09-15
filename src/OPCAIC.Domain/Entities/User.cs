﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace OPCAIC.Domain.Entities
{
	/// <summary>
	///     Represents a user in the platform.
	/// </summary>
	public class User : IdentityUser<long>, IEntity, ISoftDeletable, IChangeTrackable
	{
		/// <summary>
		///     First name of the user.
		/// </summary>
		public string FirstName { get; set; }

		/// <summary>
		///     Last name of the user.
		/// </summary>
		public string LastName { get; set; }

		/// <summary>
		///     Id of the role of this user.
		/// </summary>
		public long RoleId { get; set; }

		/// <summary>
		///     The role of this user.
		/// </summary>
		public virtual Role Role { get; set; }

		/// <summary>
		///     UI language to use for the user.
		/// </summary>
		public string LocalizationLanguage { get; set; }

		/// <summary>
		///     Organization this user belongs to
		/// </summary>
		public string Organization { get; set; }

		/// <summary>
		///     Flags whether the user wishes to receive email notifications.
		/// </summary>
		public bool WantsEmailNotifications { get; set; }

		/// <summary>
		///     All submissions from this user.
		/// </summary>
		public virtual IList<Submission> Submissions { get; set; }

		/// <summary>
		///     Mapping to all tournaments this user is the manager of.
		/// </summary>
		public virtual IList<TournamentManager> ManagerOfTournaments { get; set; }

		/// <inheritdoc />
		public DateTime Created { get; set; }

		/// <inheritdoc />
		public DateTime Updated { get; set; }

		/// <inheritdoc />
		public bool IsDeleted { get; set; }
	}
}