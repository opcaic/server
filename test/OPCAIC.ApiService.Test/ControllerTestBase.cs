using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test
{
	public abstract class ControllerTestBase<TController> : ApiServiceTestBase
		where TController : ControllerBase
	{
		private readonly Lazy<TController> lazyController;

		/// <inheritdoc />
		protected ControllerTestBase(ITestOutputHelper output) : base(output)
		{
			lazyController = new Lazy<TController>(CreateController);

			HttpContext = new DefaultHttpContext();
			HttpContext.User = new ClaimsPrincipal();
			Request = new DefaultHttpRequest(HttpContext);
			Response = new DefaultHttpResponse(HttpContext);

			Services.AddTransient<TController>();
		}

		protected TController Controller => lazyController.Value;

		protected HttpRequest Request { get; }
		protected HttpResponse Response { get; }
		protected HttpContext HttpContext { get; }

		private TController CreateController()
		{
			var controller = GetService<TController>();
			controller.ControllerContext.HttpContext = HttpContext;
			return controller;
		}
	}
}