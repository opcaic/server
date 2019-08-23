using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OPCAIC.Broker;

namespace OPCAIC.ApiService.Health
{
	internal class BrokerHealthCheck : IHealthCheck
	{
		private IBroker broker;

		public BrokerHealthCheck(IBroker broker)
		{
			this.broker = broker;
		}

		/// <inheritdoc />
		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
			CancellationToken cancellationToken = new CancellationToken())
		{
			var info = await broker.GetStats();

			var status = info.Workers.Count > 0 ? HealthStatus.Healthy : HealthStatus.Unhealthy;
			var desc = $"There are {info.Workers.Count} workers connected to the backend.";

			return new HealthCheckResult(status, desc);
		}
	}
}