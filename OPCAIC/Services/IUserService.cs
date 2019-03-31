using System.Threading.Tasks;
using OPCAIC.ApiService.Security;

namespace OPCAIC.ApiService.Services
{
  public interface IUserService
  {
    Task<UserIdentity[]> GetAllAsync();
    Task<UserIdentity> Authenticate(string email, string passwordHash);
  }
}
