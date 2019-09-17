using System;

namespace OPCAIC.Common
{
	public interface ITimeService
	{
		DateTime Today { get; }
		DateTime Now { get; }
	}
}