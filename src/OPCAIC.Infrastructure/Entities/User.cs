using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Represents a user in the platform.
	/// </summary>
	public class User : SoftDeletableEntity
	{
		/// <summary>
		///     Email of the user.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.UserEmail)]
		public string Email { get; set; }

		/// <summary>
		///		Nickname chosen by user.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		///     First name of the user.
		/// </summary>
		[MaxLength(StringLengths.UserFirstName)]
		public string FirstName { get; set; }

		/// <summary>
		///     Last name of the user.
		/// </summary>
		[MaxLength(StringLengths.UserLastName)]
		public string LastName { get; set; }

		/// <summary>
		///     Hash of the password.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.PasswordHash)]
		public string PasswordHash { get; set; }

		/// <summary>
		///     Flag whether the <see cref="Email" /> has been verified.
		/// </summary>
		public bool EmailVerified { get; set; }

		/// <summary>
		///     Id of the role of this user.
		/// </summary>
		public long RoleId { get; set; }

		public string LocalizationLanguage { get; set; }

		public string Organization { get; set; }

		/// <summary>
		///     The role of this user.
		/// </summary>
		[ForeignKey(nameof(RoleId))]
		public virtual UserRole Role { get; set; }

		/// <summary>
		///     All submissions from this user.
		/// </summary>
		public virtual IList<Submission> Submissions { get; set; }
	}
}