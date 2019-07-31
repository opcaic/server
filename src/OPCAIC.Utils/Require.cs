using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace OPCAIC.Utils
{
	/// <summary>
	///   Class serving for compact assertions/exception throwing useful for checking method arguments
	/// </summary>
	public static class Require
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArgNotNull(object arg, string name)
			=> That<ArgumentNullException>(arg != null, name);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NotEmpty<T>(IEnumerable collection, string name) where T : Exception
			=> That<T>(collection.GetEnumerator().MoveNext(), name);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArgInRange(int i, int itemCount, string name)
		{
			Debug.Assert(itemCount >= 0);
			That<ArgumentOutOfRangeException>(i >= 0 && i < itemCount, name);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FileExists(string filename)
			=> FileExists(filename, $"File {filename} does not exist.");

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FileExists(string filename, string message)
		{
			Debug.Assert(filename != null);
			That<InvalidOperationException>(!File.Exists(filename), message);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Nonnegative(int value, string name)
			=> That<ArgumentOutOfRangeException>(value >= 0, name);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void That<T>(bool condition, string message) where T : Exception 
			=> That<T>(condition, (object) message);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void That<T>(bool condition, FormattableString message) where T : Exception
			=> That<T>(condition, message.ToString());

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void That<T>(bool condition, params object[] ctorArgs) where T : Exception
		{
			Debug.Assert(ctorArgs != null);
			if (!condition)
			{
				ThrowHelper<T>(ctorArgs);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ThrowHelper<T>(object[] ctorArgs) where T : Exception
			=> throw ((T) Activator.CreateInstance(typeof(T), ctorArgs));
	}
}
