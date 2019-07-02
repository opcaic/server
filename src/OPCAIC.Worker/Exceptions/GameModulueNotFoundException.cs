using System;
using System.Runtime.Serialization;

namespace OPCAIC.Worker.Exceptions
{
	[Serializable]
	public class GameModulueNotFoundException : InvalidOperationException
	{
		public GameModulueNotFoundException(string moduleName) : base(
			$"GameModule '{moduleName}' does not exist.")
		{
		}

		protected GameModulueNotFoundException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}
