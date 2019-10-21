using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using OPCAIC.Domain.Exceptions;
using OPCAIC.Utils;

namespace OPCAIC.Domain.Infrastructure
{
	/// <summary>
	///     Helper methods for <see cref="Enumeration{T}" /> class.
	/// </summary>
	public abstract class Enumeration : IEquatable<Enumeration>
	{
		private static readonly Dictionary<Type, EnumerationType> types =
			new Dictionary<Type, EnumerationType>();

		/// <summary>
		///     Name of the enumeration value.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		///     Unique id of the enumeration value.
		/// </summary>
		public int Id { get; private set; }

		/// <inheritdoc />
		public bool Equals(Enumeration other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Name == other.Name && Id == other.Id;
		}

		protected static T Create<T>([CallerMemberName] string name = null)
			where T : Enumeration<T>, new()
		{
			return CreateInternal<T, T>(name);
		}

		protected static T Create<T>(int id, [CallerMemberName] string name = null)
			where T : Enumeration<T>, new()
		{
			return CreateInternal<T, T>(id, name);
		}

		/// <summary>
		///     Returns all values of enumeration of given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<Enumeration> GetAllValues(Type type)
		{
			return GetEnumerationType(type).AllValues;
		}

		/// <summary>
		///     Tries to find the enumeration value identified by given id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryFromId<T>(int id, out T value)
			where T : Enumeration<T>
		{
			value = (T)GetEnumerationType(typeof(T)).LookupFromId.GetValueOrDefault(id);
			return value != null;
		}

		/// <summary>
		///     Tries to find the enumeration value identified by given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryFromName<T>(string name, out T value)
			where T : Enumeration<T>
		{
			value = (T)GetEnumerationType(typeof(T)).LookupFromName.GetValueOrDefault(name);
			return value != null;
		}

		private static EnumerationType GetEnumerationType(Type type)
		{
			if (!types.TryGetValue(type, out var enumType))
			{
				types[type] = enumType = new EnumerationType(type);
			}

			return enumType;
		}

		private static int GetNextId<T>()
		{
			var type = GetEnumerationType(typeof(T));

			if (type.AllValues.Count == 0)
			{
				return 1; // start from one
			}

			return type.AllValues[^1].Id + 1;
		}

		protected static U CreateInternal<T, U>([CallerMemberName] string name = null)
			where T : Enumeration<T>
			where U : T, new()
		{
			return CreateInternal<T, U>(GetNextId<T>(), name);
		}

		protected static U CreateInternal<T, U>(int id, [CallerMemberName] string name = null)
			where T : Enumeration<T>
			where U : T, new()
		{
			var type = GetEnumerationType(typeof(T));

			Require.That<EnumerationException>(!type.LookupFromName.ContainsKey(name),
				$"Enumeration {typeof(T).Name} already defines value with name '{name}'");
			Require.That<EnumerationException>(!type.LookupFromId.ContainsKey(id),
				$"Enumeration {typeof(T).Name} already defines value with id '{id}'");

			var enumeration = new U {Id = id, Name = name};
			type.AllValues.Add(enumeration);
			type.LookupFromName[name] = enumeration;
			type.LookupFromId[id] = enumeration;
			return enumeration;
		}

		/// <summary>
		///     Returns true if given type is a type of an enumeration
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static bool IsEnumerationType(Type type)
		{
			return type.BaseType?.IsGenericType == true &&
				type.BaseType.GetGenericTypeDefinition() == typeof(Enumeration<>);
		}

		/// <summary>
		///     Returns all enumeration types in the current <see cref="AppDomain" />;
		/// </summary>
		/// <returns></returns>
		public static IEnumerable<Type> GetAllEnumerationTypes()
		{
			return AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic)
				.SelectMany(t => t.ExportedTypes)
				.Where(IsEnumerationType);
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

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((Enumeration)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			return Id;
		}

		public static bool operator ==(Enumeration left, Enumeration right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Enumeration left, Enumeration right)
		{
			return !Equals(left, right);
		}

		private class EnumerationType
		{
			public EnumerationType(Type type)
			{
				Type = type;
				LookupFromName = new Dictionary<string, Enumeration>();
				LookupFromId = new Dictionary<int, Enumeration>();
				AllValues = new List<Enumeration>();
			}

			public Type Type { get; }
			public Dictionary<string, Enumeration> LookupFromName { get; }
			public Dictionary<int, Enumeration> LookupFromId { get; }
			public List<Enumeration> AllValues { get; }
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return Name;
		}
	}

	/// <summary>
	///     Helper class for strongly typed enumerations.
	/// </summary>
	/// <typeparam name="T">Type of the enumeration class.</typeparam>
	[DebuggerDisplay("{Name} [{Id}]")]
	public abstract class Enumeration<T> : Enumeration
		where T : Enumeration<T>
	{
		static Enumeration()
		{
			// make sure all static fields are initialized
			RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
		}

		protected static U CreateDerived<U>([CallerMemberName] string name = null)
			where U : T, new()
		{
			return CreateInternal<T, U>(name);
		}

		protected static U CreateDerived<U>(int id, [CallerMemberName] string name = null)
			where U : T, new()
		{
			return CreateInternal<T, U>(id, name);
		}

		/// <summary>
		///     Contains all the enumeration values;
		/// </summary>
		public static IEnumerable<T> AllValues => GetAllValues(typeof(T)).OfType<T>();

		/// <summary>
		///     Gets enumeration with given id value.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static T FromId(int id)
		{
			if (!TryFromId(id, out var value))
				throw new EnumerationException(
					$"No enumeration {typeof(T).Name} with id {id} exists.");

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
				throw new EnumerationException(
					$"No enumeration {typeof(T).Name} with name '{name}' exists.");

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
			return TryFromId<T>(id, out value);
		}

		/// <summary>
		///     Tries to find the enumeration value identified by given name.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool TryFromName(string name, out T value)
		{
			return TryFromName<T>(name, out value);
		}
	}
}