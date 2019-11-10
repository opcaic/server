using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Test;
using OPCAIC.TestUtils;
using Xunit.Abstractions;
using ValidationContext = FluentValidation.ValidationContext;

namespace OPCAIC.Application.Test
{
	public abstract class HandlerTest<THandler> : ServiceTestBase
		where THandler : class
	{
		protected EntityFaker Faker { get; }
		private THandler handler;
		protected THandler Handler => handler ??= GetService<THandler>();

		protected CancellationToken CancellationToken => CancellationToken.None;

		protected Task<TResponse> Send<TRequest, TResponse>(TRequest request)
			where TRequest : IRequest<TResponse>
			=> ((IRequestHandler<TRequest, TResponse>)Handler).Handle(request,
				CancellationToken);

		/// <inheritdoc />
		protected HandlerTest(ITestOutputHelper output) : base(output)
		{
			Faker = new EntityFaker();

			Services.AddSingleton(TestMapper.Mapper);
			Services.AddTransient<THandler>();
			Services.AddTransient<THandler>();
			Services.AddValidatorsFromAssemblyContaining<THandler>();
		}

		protected ValidationResult Validate<T>(T instance)
		{
			var validator = ServiceProvider.GetRequiredService<IValidator<T>>();

			// prepare context so dependency injection works
			var ctx = new ValidationContext(instance);
			ctx.SetServiceProvider(ServiceProvider);

			return validator.Validate(ctx);
		}
	}
}