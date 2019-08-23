using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Security;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.TestUtils;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test
{
	public abstract class ApiServiceTestBase : ServiceTestBase
	{
		/// <inheritdoc />
		protected ApiServiceTestBase(ITestOutputHelper output) : base(output)
		{
			Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false)
				.Build();
			CancellationTokenSource = new CancellationTokenSource();

			var startup = new Startup(Configuration);
			Services.ConfigureSecurity(Configuration);
			startup.ConfigureOptions(Services);

			UseDatabase();
			Services.AddServices();
			Services.AddBroker();
			Services.AddRepositories();
			Services.AddMapper();

			// make sure no email gets actually sent
			EmailServiceMock = Services.Mock<IEmailService>();

			lazyDbContext = new Lazy<DataContext>(GetService<DataContext>);
		}

		protected IConfiguration Configuration { get; }
		protected CancellationTokenSource CancellationTokenSource { get; }
		protected CancellationToken CancellationToken => CancellationTokenSource.Token;
		protected Mock<IEmailService> EmailServiceMock { get; }

		private readonly Lazy<DataContext> lazyDbContext;
		protected DataContext DbContext => lazyDbContext.Value;
		protected void TurnOffAuthorization()
		{
			var mock = Services.Mock<IAuthorizationService>();

			// setup both methods to return success no matter what
			mock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(),
				It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
			mock.Setup(s => s.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(),
					It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
				.ReturnsAsync(AuthorizationResult.Success());
		}
	}
}