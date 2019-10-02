using System;
using System.Runtime.Serialization;

namespace OPCAIC.Domain.Exceptions
{
	[Serializable]
	public class EnumerationException : Exception
	{
		public EnumerationException(string message) : base(message) { }

		protected EnumerationException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}