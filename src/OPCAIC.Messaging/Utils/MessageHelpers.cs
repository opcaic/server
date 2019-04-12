using System.Text;
using NetMQ;
using Newtonsoft.Json;

namespace OPCAIC.Messaging.Utils
{
	
	public static class MessageHelpers
	{
		private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Auto
		};

		public static void AppendIdentity(this NetMQMessage msg, string identity)
			=> msg.Append(IdentityToBytes(identity));

		public static string ConvertToIdentity(this NetMQFrame frame)
			=> BytesToIdentity(frame.Buffer, 0, frame.BufferSize);

	// TODO: use binary serialization? (using JSON now for debug readability)
		public static void SerializeMessage(NetMQMessage msg, object payload)
			// using typeof(object) forces the serializer to include the type name of the root object
			=> msg.Append(JsonConvert.SerializeObject(payload, typeof(object), serializerSettings));

		public static object DeserializeMessage(NetMQMessage msg)
			=> JsonConvert.DeserializeObject(msg.Pop().ConvertToString(), serializerSettings);

		public static string BytesToIdentity(byte[] buffer, int start, int count)
			=> Encoding.Default.GetString(buffer, start, count);

		public static byte[] IdentityToBytes(string id) => Encoding.Default.GetBytes(id);
	}
}
