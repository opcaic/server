using System;
using OPCAIC.Common;

namespace OPCAIC.Infrastructure
{
	public class MachineTimeService : ITimeService
	{
		/// <inheritdoc />
		public DateTime Today => DateTime.Today.ToUniversalTime();

		/// <inheritdoc />
		public DateTime Now => DateTime.UtcNow;
	}
}