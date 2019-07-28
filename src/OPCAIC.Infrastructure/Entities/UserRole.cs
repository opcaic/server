using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     A user role.
	/// </summary>
	public class UserRole : EntityBase
	{
		/// <summary>
		///     Name of the role.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.UserRoleName)]
		public string Name { get; set; }

		internal void OnModelCreating(EntityTypeBuilder<UserRole> builder)
		{
			builder.HasIndex(r => r.Name).IsUnique();
		}
	}
}