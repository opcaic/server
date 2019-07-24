using System;
using System.Runtime.Serialization;

namespace OPCAIC.Worker.GameModules
{
	[Serializable]
	public class GameModuleException : Exception
	{
		public GameModuleException() { }
		public GameModuleException(string message) : base(message) { }
		public GameModuleException(string message, Exception inner) : base(message, inner) { }

		protected GameModuleException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}

	[Serializable]
	class GameModuleProcessStartException : GameModuleException
	{
		public GameModuleProcessStartException(string message) : base(message) { }
		protected GameModuleProcessStartException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}