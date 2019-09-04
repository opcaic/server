using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Security;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.TestUtils;
using Xunit;
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

			lazyDbContext = new Lazy<DataContext>(GetService<DataContext>);
		}

		protected void ApiConfigureServices()
		{
			var startup = new Startup(Configuration);
			Services.ConfigureSecurity(Configuration);
			startup.ConfigureOptions(Services);

			UseDatabase();
			Services.AddServices();
			Services.AddBroker();
			Services.AddRepositories();
			Services.AddMapper();

		}

		protected IConfiguration Configuration { get; }
		protected CancellationTokenSource CancellationTokenSource { get; }
		protected CancellationToken CancellationToken => CancellationTokenSource.Token;

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

		protected async Task AssertUnauthorized(string errorCode, Func<Task> testCode)
		{
			var ex = await Assert.ThrowsAsync<UnauthorizedException>(testCode);
			Assert.Equal(errorCode, ex.Code);
		}

		protected async Task AssertNotFound<TEntity>(long id, Func<Task> testCode)
		{
			var ex = await Assert.ThrowsAsync<NotFoundException>(testCode);
			Assert.Equal(id, ex.ResourceId);
			Assert.Equal(typeof(TEntity).Name, ex.Resource);
		}

		protected async Task AssertModelValidationException<TEx>(int statusCode, string errorCode,
			string field, Func<Task> testCode)
			where TEx : ModelValidationException
		{
			var ex = await Assert.ThrowsAsync<TEx>(testCode);
			Assert.Equal(statusCode, ex.StatusCode);

			var error = Assert.Single(ex.ValidationErrors);
			Assert.Equal(errorCode, error.Code);
			Assert.Equal(field, error.Field);
		}

		protected Task AssertInvalidModel(string errorCode, string field, Func<Task> testCode)
		{
			return AssertModelValidationException<ModelValidationException>(
				StatusCodes.Status400BadRequest, errorCode, field, testCode);
		}

		protected Task AssertBadRequest(string errorCode, string field, Func<Task> testCode)
		{
			return AssertModelValidationException<BadRequestException>(
				StatusCodes.Status400BadRequest, errorCode, field, testCode);
		}

		protected Task AssertConflict(string errorCode, string field, Func<Task> testCode)
		{
			return AssertModelValidationException<ConflictException>(
				StatusCodes.Status409Conflict, errorCode, field, testCode);
		}
	}
}