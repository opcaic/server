using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService
{
  public class BadRequestException: ApiException
  {
    public BadRequestException(string message)
      : base(StatusCodes.Status400BadRequest, message) { }
  }
}
