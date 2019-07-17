using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPCAIC.Infrastructure.Entities
{
  public class User: Entity
  {
    public string Email { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PasswordHash { get; set; }

    public long RoleId { get; set; }

    public bool EmailVerified { get; set; }

    [ForeignKey(nameof(RoleId))]
    public UserRole Role { get; set; }
  }
}
