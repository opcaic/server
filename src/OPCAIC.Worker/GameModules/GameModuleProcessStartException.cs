using System;
using System.Runtime.Serialization;
using OPCAIC.GameModules.Interface;

namespace OPCAIC.Worker.GameModules
{
	[Serializable]
	internal class GameModuleProcessStartException : GameModuleException
	{
		public GameModuleProcessStartException(string message) : base(message) { }

		protected GameModuleProcessStartException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}