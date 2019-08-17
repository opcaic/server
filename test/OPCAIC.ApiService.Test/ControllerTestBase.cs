using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Services;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.TestUtils;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test
{
	public abstract class ControllerTestBase<TController> : ServiceTestBase
		where TController : ControllerBase
	{
		private readonly Lazy<TController> lazyController;

		/// <inheritdoc />
		protected ControllerTestBase(ITestOutputHelper output) : base(output)
		{
			Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false)
				.Build();
			CancellationTokenSource = new CancellationTokenSource();
			lazyController = new Lazy<TController>(CreateController);

			HttpContext = new DefaultHttpContext();
			Request = new DefaultHttpRequest(HttpContext);
			Response = new DefaultHttpResponse(HttpContext);

			var startup = new Startup(Configuration);
			startup.ConfigureSecurity(Services);
			startup.ConfigureOptions(Services);

			// random new name so tests can run in parallel
			var dbName = Guid.NewGuid().ToString();
			Services.AddDbContext<DataContext>(options =>
			{
				options.UseInMemoryDatabase(dbName);
				options.EnableSensitiveDataLogging();
				options.EnableDetailedErrors();
			});

			Services.AddServices();
			Services.AddBroker();
			Services.AddRepositories();
			Services.AddMapper();

			Services.AddTransient<TController>();

			// make sure no email gets actually sent
			EmailServiceMock = Services.Mock<IEmailService>();
		}

		private TController CreateController()
		{
			var controller = GetService<TController>();
			controller.ControllerContext.HttpContext = HttpContext;
			return controller;
		}

		protected TController Controller => lazyController.Value;
		protected IConfiguration Configuration { get; }
		protected CancellationTokenSource CancellationTokenSource { get; }
		protected CancellationToken CancellationToken => CancellationTokenSource.Token;

		protected Mock<IEmailService> EmailServiceMock { get; }

		protected HttpRequest Request { get; }
		protected HttpResponse Response { get; }
		protected HttpContext HttpContext { get; }
	}
}