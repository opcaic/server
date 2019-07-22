using System;
using System.Runtime.Serialization;

namespace OPCAIC.Worker.Exceptions
{
	[Serializable]
	public class GameModuleNotFoundException : InvalidOperationException
	{
		public GameModuleNotFoundException(string moduleName) : base(
			$"GameModule '{moduleName}' does not exist.")
		{
		}

		protected GameModuleNotFoundException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}
