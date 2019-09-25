using System;
using AutoMapper;
using OPCAIC.Domain.Enums;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Application.Infrastructure.AutoMapper.Converters
{
	public class JobStatusToWorkerJobStateConverter : ITypeConverter<JobStatus, WorkerJobState>
	{
		/// <inheritdoc />
		public WorkerJobState Convert(JobStatus source, WorkerJobState destination,
			ResolutionContext context)
		{
			switch (source)
			{
				case JobStatus.Ok:
				case JobStatus.Timeout:
				case JobStatus.Error:
					return WorkerJobState.Finished;

				case JobStatus.Canceled:
					return WorkerJobState.Cancelled;

				case JobStatus.Unknown:
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, null);
			}
		}
	}
}