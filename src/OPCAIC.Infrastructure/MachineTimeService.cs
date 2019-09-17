using System;
using OPCAIC.Common;

namespace OPCAIC.Infrastructure
{
	public class MachineTimeService : ITimeService
	{
		/// <inheritdoc />
		public DateTime Today => DateTime.Today;

		/// <inheritdoc />
		public DateTime Now => DateTime.Now;
	}
}