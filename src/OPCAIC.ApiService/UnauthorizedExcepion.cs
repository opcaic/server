using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService
{
  public class UnauthorizedExcepion: ApiException
  {
    public UnauthorizedExcepion(string message)
      :base(StatusCodes.Status401Unauthorized, message) { }
  }
}
