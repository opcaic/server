using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using OPCAIC.Domain.Enumerations;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Domain.Entities
{
	/// <summary>
	///     Represents a user in the platform.
	/// </summary>
	public class User : IdentityUser<long>, IEntity, IChangeTrackable
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
		///     The role of this user.
		/// </summary>
		public UserRole Role { get; set; }

		/// <summary>
		///     UI language to use for the user.
		/// </summary>
		public LocalizationLanguage LocalizationLanguage { get; set; }

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
		///     Link to participations to all tournaments this user participates in.
		/// </summary>
		public virtual IList<TournamentParticipation> TournamentParticipations { get; set; }

		/// <summary>
		///     All tournaments this user has created.
		/// </summary>
		public virtual IList<Tournament> OwnedTournaments { get; set; }

		/// <summary>
		///     Mapping to all tournaments this user is the manager of.
		/// </summary>
		public virtual IList<TournamentManager> ManagerOfTournaments { get; set; }

		/// <inheritdoc />
		public DateTime Created { get; set; }

		/// <inheritdoc />
		public DateTime Updated { get; set; }
	}
}