using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using OPCAIC.Domain.Exceptions;
using OPCAIC.Utils;

namespace OPCAIC.Domain.Infrastructure
{
	/// <summary>
	///     Helper class for strongly typed enumerations.
	/// </summary>
	/// <typeparam name="T">Type of the enumeration class.</typeparam>
	[DebuggerDisplay("{Name} [{Id}]")]
	public abstract class Enumeration<T> : IEquatable<T>
		where T : Enumeration<T>, new()
	{
		private static readonly Dictionary<string, T> lookupFromName = new Dictionary<string, T>();
		private static readonly Dictionary<int, T> lookupFromId = new Dictionary<int, T>();
		private static readonly List<T> allValues = new List<T>();

		static Enumeration()
		{
			// make sure all static fields are initialized
			RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
		}

		/// <summary>
		///     Contains all the enumeration values;
		/// </summary>
		public static IReadOnlyList<T> AllValues => allValues;

		/// <summary>
		///     Name of the enumeration value.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///     Unique id of the enumeration value.
		/// </summary>
		public int Id { get; private set; }

		/// <inheritdoc />
		public bool Equals(T other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Id == other.Id;
		}

		private static int GetNextId()
		{
			if (allValues.Count == 0)
			{
				return 0; // start from zero
			}

			return allValues[allValues.Count - 1].Id + 1;
		}

		protected static T Create([CallerMemberName] string name = null)
		{
			return Create(GetNextId(), name);
		}

		protected static T Create(int id, [CallerMemberName] string name = null)
		{
			Require.That<EnumerationException>(!lookupFromName.ContainsKey(name),
				$"Enumeration {typeof(T).Name} already defines value with name '{name}'");
			Require.That<EnumerationException>(!lookupFromId.ContainsKey(id),
				$"Enumeration {typeof(T).Name} already defines value with id '{id}'");

			var enumeration = new T {Id = id, Name = name};
			allValues.Add(enumeration);
			lookupFromName[name] = enumeration;
			lookupFromId[id] = enumeration;
			return enumeration;
		}

		/// <summary>
		///     Gets enumeration with given id value.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static T FromId(int id)
		{
			if (!TryFromId(id, out var value))
				throw new EnumerationException($"No enumeration {typeof(T).Name} with id {id} exists.");

			return value;
		}

		/// <summary>
		///     Gets enumeration with given id value.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T FromName(string name)
		{
			if (!TryFromName(name, out var value))
				throw new EnumerationException($"No enumeration {typeof(T).Name} with name '{name}' exists.");

			return value;
		}

		/// <summary>
		///     Tries to find the enumeration value identified by given id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryFromId(int id, out T value)
		{
			return lookupFromId.TryGetValue(id, out value);
		}

		/// <summary>
		///     Tries to find the enumeration value identified by given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryFromName(string name, out T value)
		{
			return lookupFromName.TryGetValue(name, out value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((Enumeration<T>)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Id;
		}

		public static bool operator ==(Enumeration<T> left, Enumeration<T> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Enumeration<T> left, Enumeration<T> right)
		{
			return !Equals(left, right);
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Name;
		}
	}
}