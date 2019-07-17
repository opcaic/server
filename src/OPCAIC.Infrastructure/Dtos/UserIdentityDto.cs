namespace OPCAIC.Infrastructure.Dtos
{
  public class UserIdentityDto
  {
    public long Id { get; set; }

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string Token { get; set; }

    public long RoleId { get; set; }
  }
}
