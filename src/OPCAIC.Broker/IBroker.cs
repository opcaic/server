using System;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker
{
	/// <summary>
	///   Broker for the load-balanced task distribution.
	/// </summary>
	public interface IBroker
	{
		/// <summary>
		///   Enqueues a work item to be dispatched on a worker.
		/// </summary>
		/// <param name="msg"></param>
		void EnqueueWork(WorkMessageBase msg);

		/// <summary>
		///   Gets number of scheduled but not finished tasks.
		/// </summary>
		/// <returns></returns>
		int GetUnfinishedTasksCount();

		/// <summary>
		///   Starts the socket and consumer thread for the broker.
		/// </summary>
		void StartBrokering();

		/// <summary>
		///   Stops the broker.
		/// </summary>
		void StopBrokering();

		/// <summary>
		///   Registers a handler for the messages sent from the worker.
		/// </summary>
		/// <typeparam name="TMessage">Type of the message</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterHandler<TMessage>(Action<TMessage> handler);
	}
}
