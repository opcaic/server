using System;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Defines fields needed in order to manage the job execution.
	/// </summary>
	public interface IWorkerJob
	{
		/// <summary>
		///     Id of the job which is responsible for the job.
		/// </summary>
		Guid JobId { get; set; }

		/// <summary>
		///     The state this job is in. 
		/// </summary>
		WorkerJobState State { get; set; }

		/// <summary>
		///     Timestamp when the job was finished.
		/// </summary>
		DateTime? Finished { get; set; }
	}
}