using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OPCAIC.Worker.Services
{
	public static class JsonHelper
	{
		private class StrictContractResolver : DefaultContractResolver
		{
			/// <inheritdoc />
			protected override JsonProperty CreateProperty(MemberInfo member,
				MemberSerialization memberSerialization)
			{
				var prop = base.CreateProperty(member, memberSerialization);

				// make sure all are present
				prop.Required = Required.Always;
				return prop;
			}
		}
		private static readonly JsonSerializerSettings strictSettings =
			new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Error,
				ContractResolver = new StrictContractResolver(),
				CheckAdditionalContent = true
			};

		private static readonly JsonSerializerSettings withExtraSettings =
			new JsonSerializerSettings
			{
				ContractResolver = new StrictContractResolver(),
				CheckAdditionalContent = true
			};


		/// <summary>
		///     Deserializes given JSON string to an instance of given type. Properties on the
		///     target type and JSON must be matchable one-to-one, no missing or extra properties
		///     are allowed.
		/// </summary>
		/// <typeparam name="T">Type of the target object.</typeparam>
		/// <param name="json">The JSON to deserialize.</param>
		/// <returns></returns>
		public static T DeserializeStrict<T>(string json) => JsonConvert.DeserializeObject<T>(json, strictSettings);

		/// <summary>
		///     Deserializes given JSON string to an instance of given type. All properties on the
		///     target type <see cref="T"/> must be present in the JSON. Extra properties are
		///     allowed.
		/// </summary>
		/// <typeparam name="T">Type of the target object.</typeparam>
		/// <param name="json">The JSON to deserialize.</param>
		/// <returns></returns>
		public static T DeserializeWithExtra<T>(string json) => JsonConvert.DeserializeObject<T>(json, withExtraSettings);
	}
}