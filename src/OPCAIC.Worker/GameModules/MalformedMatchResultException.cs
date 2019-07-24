using System;
using System.Runtime.Serialization;

namespace OPCAIC.Worker.GameModules
{
	[Serializable]
	public class MalformedMatchResultException : GameModuleException
	{
		public MalformedMatchResultException(string message) : base(message) { }
		public MalformedMatchResultException(string message, Exception inner) : base(message, inner) { }

		protected MalformedMatchResultException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}