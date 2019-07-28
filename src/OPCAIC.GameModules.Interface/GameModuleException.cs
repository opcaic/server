using System;
using System.Runtime.Serialization;

namespace OPCAIC.GameModules.Interface
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
}