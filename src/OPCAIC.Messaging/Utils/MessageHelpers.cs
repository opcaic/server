using System.Text;
using NetMQ;
using Newtonsoft.Json;

namespace OPCAIC.Messaging.Utils
{
	public static class MessageHelpers
	{
		public static void AppendIdentity(this NetMQMessage msg, string identity)
			=> msg.Append(IdentityToBytes(identity));

		public static string ConvertToIdentity(this NetMQFrame frame)
			=> BytesToIdentity(frame.Buffer, 0, frame.BufferSize);

		public static void SerializeMessage(NetMQMessage msg, object payload)
			=> msg.Append(JsonConvert.SerializeObject(payload, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			}));

		public static object DeserializeMessage(NetMQMessage msg)
			=> JsonConvert.DeserializeObject(msg.Pop().ConvertToString(), new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});

		public static string BytesToIdentity(byte[] buffer, int start, int count)
			=> Encoding.Default.GetString(buffer, start, count);

		public static byte[] IdentityToBytes(string id) => Encoding.Default.GetBytes(id);
	}
}
