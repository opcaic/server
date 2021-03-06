﻿using System;
using System.Runtime.Serialization;
using OPCAIC.GameModules.Interface;

namespace OPCAIC.Worker.Exceptions
{
	[Serializable]
	public class MalformedMatchResultException : GameModuleException
	{
		public MalformedMatchResultException(string message) : base(message) { }

		public MalformedMatchResultException(string message, Exception inner) : base(message, inner)
		{
		}

		protected MalformedMatchResultException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}