using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker
{
	/// <summary>
	///     Broker for the load-balanced task distribution.
	/// </summary>
	public interface IBroker
	{
		/// <summary>
		///     Returns information about current state of the backend.
		/// </summary>
		/// <returns></returns>
		Task<BrokerStats> GetStats();

		/// <summary>
		///     Enqueues a work item to be dispatched on a worker.
		/// </summary>
		/// <param name="msg"></param>
		Task EnqueueWork(WorkMessageBase msg);

		/// <summary>
		///     Moves work item with given id to the start of the queue.
		/// </summary>
		/// <param name="id">Id of the job to prioritize.</param>
		Task<bool> PrioritizeWork(Guid id);

		/// <summary>
		///     Get all work items in the queue.
		/// </summary>
		/// <returns></returns>
		Task<List<WorkItem>> GetWorkItems();

		/// <summary>
		///     Cancels work job with given id.
		/// </summary>
		/// <param name="id">Id of the job to cancel.</param>
		Task<bool> CancelWork(Guid id);


		/// <summary>
		///     Gets number of scheduled but not finished tasks.
		/// </summary>
		/// <returns></returns>
		Task<int> GetUnfinishedTasksCount();

		/// <summary>
		///     Starts the socket and consumer thread for the broker.
		/// </summary>
		Task StartBrokering();

		/// <summary>
		///     Stops the broker. Waits until workers finish up untill the cancellation token requests
		///     immediate termination.
		/// </summary>
		/// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
		Task StopBrokering(CancellationToken cancellationToken);

		/// <summary>
		///     Registers a handler for the messages sent from the worker.
		/// </summary>
		/// <typeparam name="TMessage">Type of the message</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterHandler<TMessage>(Action<TMessage> handler);
	}
}