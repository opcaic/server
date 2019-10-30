using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Broker;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Persistence;
using OPCAIC.TestUtils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test
{
	public abstract class ApiServiceTestBase : ServiceTestBase
	{
		private readonly Lazy<DataContext> lazyDbContext;

		private readonly Lazy<IMapper> lazyMapper;

		/// <inheritdoc />
		protected ApiServiceTestBase(ITestOutputHelper output) : base(output)
		{
			Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false)
				.Build();
			CancellationTokenSource = new CancellationTokenSource();
			Services.Mock<ITimeService>().SetupGet(g => g.Now).Returns(DateTime.UtcNow);

			lazyDbContext = GetLazyService<DataContext>();
			lazyMapper = GetLazyService<IMapper>();
		}

		protected IConfiguration Configuration { get; }
		protected CancellationTokenSource CancellationTokenSource { get; }
		protected CancellationToken CancellationToken => CancellationTokenSource.Token;

		protected EntityFaker Faker { get; } = new EntityFaker();
		protected DataContext DbContext => lazyDbContext.Value;
		protected IMapper Mapper => lazyMapper.Value;

		protected void ApiConfigureServices()
		{
			var mock = new Mock<IWebHostEnvironment>();
			var startup = new Startup(Configuration, mock.Object, NullLogger<Startup>.Instance);
			Services.ConfigureSecurity(Configuration, NullLogger.Instance);
			startup.ConfigureOptions(Services);

			UseDatabase();
			Services.AddServices();
			Services.AddBroker();
			Services.AddRepositories();
			Services.AddMapper();
		}

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

			var error = ex.ValidationErrors.ShouldHaveSingleItem();
			var valError = error as ValidationError;
			valError.Code.ShouldBe(errorCode);
			valError.Field.ShouldBe(field);
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

		protected async Task AssertConflict(string errorCode, string field, Func<Task> testCode)
		{
			var ex = await Assert.ThrowsAsync<ConflictException>(testCode);

			var valError = ex.Error.ShouldBeOfType<ConflictException.ConflictError>();
			valError.Code.ShouldBe(errorCode);
			valError.Field.ShouldBe(field);
		}

		protected T NewTrackedEntity<T>() where T : class
		{
			var entity = Faker.Entity<T>();
			DbContext.Set<T>().Add(entity);
			return entity;
		}

		protected User NewUser()
		{
			var user = Faker.Entity<User>();
			GetService<UserManager>().CreateAsync(user, "pass67S#@#$@").Wait();
			return user;
		}
	}
}