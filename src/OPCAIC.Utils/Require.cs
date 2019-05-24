using System;
using System.Collections;
using System.Diagnostics;

namespace OPCAIC.Utils
{
	/// <summary>
	///   Class serving for compact assertions/exception throwing useful for checking method arguments
	/// </summary>
	public static class Require
	{
		public static void NotNull(object arg, string name)
		{
			Debug.Assert(name != null);
			That<ArgumentNullException>(arg != null, name);
		}

		public static void NotEmpty<T>(IEnumerable collection, string name) where T : Exception
		{
			Debug.Assert(name != null);
			That<T>(collection.GetEnumerator().MoveNext(), name);
		}

		public static void IndexInRange(int i, int itemCount, string name)
		{
			Debug.Assert(name != null);
			Debug.Assert(itemCount >= 0);
			That<ArgumentOutOfRangeException>(i >= 0 && i < itemCount, name);
		}

		public static void That<T>(bool condition, string message) where T : Exception
		{
			Debug.Assert(message != null);
			if (!condition)
			{
				ThrowHelper<T>(message);
			}
		}

		private static void ThrowHelper<T>(string arg) where T : Exception
			=> throw ((T) Activator.CreateInstance(typeof(T), arg));
	}
}
