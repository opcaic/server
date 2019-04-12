using NetMQ;
using Newtonsoft.Json;

namespace OPCAIC.Messaging.Utils
{
	public static class MessageHelpers
	{
		public static void SerializeMessage(NetMQMessage msg, object payload)
		{
//			var bf = new BinaryFormatter();
//			var ms = new MemoryStream();
//			bf.Serialize(ms, payload);
//			msg.Append(new NetMQFrame(ms.GetBuffer(), (int) ms.Length));
			msg.Append(JsonConvert.SerializeObject(payload, new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.All
			}));
		}

		public static object DeserializeMessage(NetMQMessage msg)
		{
//			var frame = msg.Pop();
//			var ms = new MemoryStream(frame.Buffer, 0, frame.BufferSize);
//			var bf = new BinaryFormatter();
//			return bf.Deserialize(ms);
			return JsonConvert.DeserializeObject(msg.Pop().ConvertToString(), new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.All
			});
		}
	}
}