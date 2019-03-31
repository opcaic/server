using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Security
{
  public class UserIdentity
  {
    public int Id { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string Token { get; set; }

    public UserRole Role { get; set; }
  }
}
