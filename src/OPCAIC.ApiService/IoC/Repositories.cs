using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.IoC
{
  public static class Repositories
  {
    public static void AddRepositories(this IServiceCollection serviceCollection)
    {
      serviceCollection.AddTransient<IUserRepository, UserRepository>();
    }
  }
}
