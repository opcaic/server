using System;
using AutoMapper;
using OPCAIC.Domain.Enums;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Application.Infrastructure.AutoMapper.Converters
{
	public class SubTaskResultToEntryPointResultConverter : ITypeConverter<SubTaskResult, EntryPointResult>
	{
		/// <inheritdoc />
		public EntryPointResult Convert(SubTaskResult source, EntryPointResult destination,
			ResolutionContext context)
		{
			switch (source)
			{
				case SubTaskResult.Unknown:
					return EntryPointResult.NotExecuted;
				case SubTaskResult.Ok:
					return EntryPointResult.Success;
				case SubTaskResult.NotOk:
					return EntryPointResult.UserError;
				case SubTaskResult.Aborted:
					return EntryPointResult.Cancelled;
				case SubTaskResult.ModuleError:
					return EntryPointResult.ModuleError;
				case SubTaskResult.PlatformError:
					return EntryPointResult.PlatformError;
				default:
					throw new ArgumentOutOfRangeException(nameof(source), source, null);
			}
		}
	}
}