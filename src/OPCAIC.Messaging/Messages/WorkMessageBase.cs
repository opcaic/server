using System;

namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///     Base type of messages sent from Broker to Workers.
	/// </summary>
	[Serializable]
	public class WorkMessageBase : ICloneable
	{
		/// <summary>
		///     Unique identifier of the job.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		///     Game which the work task concerns.
		/// </summary>
		public string Game { get; set; }

		/// <inheritdoc />
		public virtual object Clone()
		{
			return MemberwiseClone();
		}
	}
}