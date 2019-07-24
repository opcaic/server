using System;
using System.Threading;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker
{
	/// <summary>
	///     Broker for the load-balanced task distribution.
	/// </summary>
	public interface IBroker
	{
		/// <summary>
		///     Enqueues a work item to be dispatched on a worker.
		/// </summary>
		/// <param name="msg"></param>
		void EnqueueWork(WorkMessageBase msg);

		/// <summary>
		///     Enqueues a work item to be dispatched on a worker with set queuing time for priority
		///     scheduling.
		/// </summary>
		/// <param name="msg"></param>
		void EnqueueWork(WorkMessageBase msg, DateTime queueTime);

		/// <summary>
		///     Cancels work job with given id.
		/// </summary>
		/// <param name="id">Id of the job to cancel.</param>
		void CancelWork(Guid id);

		/// <summary>
		///     Gets number of scheduled but not finished tasks.
		/// </summary>
		/// <returns></returns>
		int GetUnfinishedTasksCount();

		/// <summary>
		///     Starts the socket and consumer thread for the broker.
		/// </summary>
		void StartBrokering();

		/// <summary>
		///     Stops the broker. Waits until workers finish up untill the cancellation token requests
		///     immediate termination.
		/// </summary>
		/// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
		void StopBrokering(CancellationToken cancellationToken);

		/// <summary>
		///     Registers a handler for the messages sent from the worker.
		/// </summary>
		/// <typeparam name="TMessage">Type of the message</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterHandler<TMessage>(Action<TMessage> handler);
	}
}