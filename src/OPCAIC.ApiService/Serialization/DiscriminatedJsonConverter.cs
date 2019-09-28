using System;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace OPCAIC.ApiService.Serialization
{
	// Adapted from https://gist.github.com/StevenLiekens/82ddcf1823ee91cf6d5edfcdb1f6a591
	public class DiscriminatedJsonConverter : JsonConverter
	{
		private readonly DiscriminatorOptions discriminatorOptions;

		public DiscriminatedJsonConverter(Type concreteDiscriminatorOptionsType)
			: this((DiscriminatorOptions)Activator.CreateInstance(concreteDiscriminatorOptionsType))
		{
		}

		public DiscriminatedJsonConverter(DiscriminatorOptions discriminatorOptions)
		{
			this.discriminatorOptions = discriminatorOptions ??
				throw new ArgumentNullException(nameof(discriminatorOptions));
		}

		public override bool CanConvert(Type objectType)
		{
			return discriminatorOptions.BaseType.IsAssignableFrom(objectType);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
			JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			var json = JObject.Load(reader);

			var propertyName = discriminatorOptions.DiscriminatorFieldName;
			// slight hack to access the inner NamingStrategy instance
			if (serializer.ContractResolver is DefaultContractResolver res)
			{
				propertyName = res.NamingStrategy.GetPropertyName(propertyName, false);
			}

			var discriminatorField = json.Property(propertyName);
			if (discriminatorField is null)
			{
				if (objectType.IsAbstract)
				{
					if (serializer.TraceWriter?.LevelFilter >= TraceLevel.Error)
					{
						serializer.TraceWriter.Trace(TraceLevel.Error,
							$"Could not find discriminator field '{propertyName}'.",
							null);
					}

					throw new JsonSerializationException(
						$"Could not find discriminator field with name '{propertyName}'.");
				}

				// use some default value so we can try instantiating the objectType
				discriminatorField = new JProperty(propertyName, null);
			}

			var discriminatorFieldValue = discriminatorField.Value.ToString();
			if (serializer.TraceWriter?.LevelFilter >= TraceLevel.Info)
			{
				serializer.TraceWriter.Trace(TraceLevel.Info,
					$"Found discriminator field '{discriminatorField.Name}' with value '{discriminatorFieldValue}'.",
					null);
			}

			var found = discriminatorOptions.DiscriminatedTypes
				.FirstOrDefault(tuple => tuple.Discriminator == discriminatorFieldValue).Type;
			if (found == null)
			{
				found = objectType;
				if (serializer.TraceWriter?.LevelFilter >= TraceLevel.Warning)
				{
					serializer.TraceWriter.Trace(TraceLevel.Warning,
						$"Discriminator value '{discriminatorFieldValue}' has no corresponding Type. Continuing anyway with Type '{objectType}'.",
						null);
				}
			}
			else
			{
				if (serializer.TraceWriter?.LevelFilter >= TraceLevel.Warning)
				{
					serializer.TraceWriter.Trace(TraceLevel.Info,
						$"Discriminator value '{discriminatorFieldValue}' was used to select Type '{found}'.",
						null);
				}
			}

			if (!discriminatorOptions.SerializeDiscriminator)
			{
				// Remove the discriminator field from the JSON for two possible reasons:
				// 1. the user doesn't want to copy the discriminator value from JSON to the CLR object, only the other way around
				// 2. the CLR object doesn't even have a discriminator property, in which case MissingMemberHandling.Error would throw
				discriminatorField.Remove();
			}

			// There might be a different converter on the 'found' type
			// Use Deserialize to let Json.NET choose the next converter
			// Use Populate to ignore any remaining converters (prevents recursion when the next converter is the same as this)
			if (found != objectType &&
				found.CustomAttributes.Any(attribute
					=> attribute.AttributeType == typeof(JsonConverterAttribute)))
			{
				return serializer.Deserialize(json.CreateReader(), found);
			}

			var value = Activator.CreateInstance(found);
			serializer.Populate(json.CreateReader(), value);
			return value;
		}

		/// <inheritdoc />
		public override bool CanWrite => false;

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// Should never happen since CanWrite is set to false;
			throw new NotSupportedException();
		}
	}
}