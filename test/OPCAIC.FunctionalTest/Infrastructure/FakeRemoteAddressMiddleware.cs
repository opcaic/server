using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace OPCAIC.FunctionalTest.Infrastructure
{
	public class FakeRemoteAddressMiddleware
	{
		private readonly RequestDelegate next;
		public static readonly string CustomHeaderName = "X-Real-IP";

		private readonly Dictionary<string, IPAddress> clientIps = new Dictionary<string, IPAddress>();

		private long counter = 0x2414188f;

		public IPAddress GetIpAddress(string id)
		{
			if (!clientIps.TryGetValue(id, out var address))
			{
				clientIps[id] = address = new IPAddress(Interlocked.Increment(ref counter));
			}

			return address;
		}

		public FakeRemoteAddressMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public Task InvokeAsync(HttpContext context)
		{
			var address = GetIpAddress(context.Request.Headers[CustomHeaderName][0]);
			context.Connection.RemoteIpAddress = address;
			context.Request.Headers[CustomHeaderName] = new StringValues(address.ToString());

			return next(context);
		}
	}
}