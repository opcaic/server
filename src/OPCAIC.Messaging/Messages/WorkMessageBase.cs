using System;
using System.Collections.Generic;

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
		public Guid JobId { get; set; }

		/// <summary>
		///     Game which the work task concerns.
		/// </summary>
		public string Game { get; set; }

		/// <summary>
		///     Game module configuration to use for the task.
		/// </summary>
		public string Configuration { get; set; }

		/// <summary>
		///     Relative url to the server where additional files for the task can be downloaded from.
		/// </summary>
		public string AdditionalFilesUri { get; set; }

		/// <summary>
		///     JWT token to be used when communicating with the server.
		/// </summary>
		public string AccessToken { get; set; }

		/// <inheritdoc />
		public virtual object Clone()
		{
			return MemberwiseClone();
		}
	}
}