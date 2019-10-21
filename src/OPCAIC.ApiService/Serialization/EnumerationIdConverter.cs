using System;
using Newtonsoft.Json;
using OPCAIC.Domain.Infrastructure;

namespace OPCAIC.ApiService.Serialization
{
	public class EnumerationIdConverter<T> : JsonConverter<T>
		where T : Enumeration<T>
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value.Id);
		}

		/// <inheritdoc />
		public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			var id = serializer.Deserialize<int>(reader);
			return Enumeration<T>.FromId(id);
		}
	}

	public class EnumerationNameConverter<T> : JsonConverter<T>
		where T : Enumeration<T>
	{
		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value.Name);
		}

		/// <inheritdoc />
		public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue,
			JsonSerializer serializer)
		{
			var name = serializer.Deserialize<string>(reader);
			if (name == null) return null;
			if (Enumeration<T>.TryFromName(name, out var value))
			{
				return value;
			}

			throw new JsonSerializationException($"Value '{name}' is not valid for {typeof(T).Name}.");
		}
	}
}