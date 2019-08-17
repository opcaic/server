using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     A user role.
	/// </summary>
	public class Role : IdentityRole<long>
	{
		internal static void OnModelCreating(EntityTypeBuilder<Role> builder)
		{
			builder.Property(r => r.Name).HasMaxLength(StringLengths.UserRoleName);
		}
	}
}