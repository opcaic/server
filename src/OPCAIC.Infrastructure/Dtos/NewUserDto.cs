namespace OPCAIC.Infrastructure.Dtos
{
  public class NewUserDto
  {
    public string Email { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PasswordHash { get; set; }

    public int RoleId { get; set; }
  }
}
