using System.ComponentModel.DataAnnotations.Schema;

namespace OPCAIC.Infrastructure.Entities
{
	public class User : Entity
	{
		public string Email { get; set; }

		public string Username { get; set; }

		public string PasswordHash { get; set; }

		public long RoleId { get; set; }

		public bool EmailVerified { get; set; }

		public string LocalizationLanguage { get; set; }

		public string Organization { get; set; }

		[ForeignKey(nameof(RoleId))]
		public UserRole Role { get; set; }
	}
}
