using System;
using System.Collections;

namespace OPCAIC.Utils
{
	/// <summary>
	///   Class serving for compact assertions/exception throwing useful for checking method arguments
	/// </summary>
	internal static class Require
	{
		public static void NotNull<T>(T arg, string name) where T : class
		{
			if (arg == null)
			{
				throw new ArgumentNullException(name);
			}
		}

		public static void NotEmpty<T>(IEnumerable collection, string name) where T : Exception
		{
			if (!collection.GetEnumerator().MoveNext())
			{
				ThrowHelper<T>(name);
			}
		}

		public static void IndexInRange(int i, int itemCount, string name)
		{
			if (i < 0 || i >= itemCount)
			{
				throw new ArgumentOutOfRangeException(name);
			}
		}

		public static void That<T>(bool condition, string message) where T : Exception
		{
			if (!condition)
			{
				ThrowHelper<T>(message);
			}
		}

		private static void ThrowHelper<T>(string arg) where T : Exception
			=> throw ((T) Activator.CreateInstance(typeof(T), arg));
	}
}
