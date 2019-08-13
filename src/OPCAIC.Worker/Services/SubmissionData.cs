using OPCAIC.Messaging.Messages;
using BotInfo = OPCAIC.GameModules.Interface.BotInfo;

namespace OPCAIC.Worker.Services
{
	public class SubmissionData
	{
		public long SubmissionId { get; set; }
		public BotInfo BotInfo { get; set; }
		public SubTaskResult CompilerResult { get; set; }
	}
}