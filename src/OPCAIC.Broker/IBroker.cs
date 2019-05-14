using System;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker
{
	public interface IBroker
	{
		void EnqueueWork(WorkMessageBase msg);
		int GetUnfinishedTasksCount();
		void StartBrokering();
		void StopBrokering();
		void RegisterHandler<TMessage>(Action<TMessage> handler);
	}
}