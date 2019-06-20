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
		{
			Debug.Assert(name != null);
			That<ArgumentNullException>(arg != null, name);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void NotEmpty<T>(IEnumerable collection, string name) where T : Exception
		{
			Debug.Assert(name != null);
			That<T>(collection.GetEnumerator().MoveNext(), name);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ArgInRange(int i, int itemCount, string name)
		{
			Debug.Assert(name != null);
			Debug.Assert(itemCount >= 0);
			That<ArgumentOutOfRangeException>(i >= 0 && i < itemCount, name);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FileExists(string filename) 
		{
			FileExists(filename, $"File {filename} does not exist.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void FileExists(string filename, string message) 
		{
			Debug.Assert(filename != null);
			Debug.Assert(message != null);
			if (!File.Exists(filename))
			{
				ThrowHelper<InvalidOperationException>(message);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void That<T>(bool condition, string message) where T : Exception
		{
			Debug.Assert(message != null);
			if (!condition)
			{
				ThrowHelper<T>(message);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void ThrowHelper<T>(string arg) where T : Exception
			=> throw ((T) Activator.CreateInstance(typeof(T), arg));
	}
}
