﻿using System;
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
		Task<BrokerStatsDto> GetStats();

		/// <summary>
		///     Enqueues a work item to be dispatched on a worker.
		/// </summary>
		/// <param name="msg"></param>
		Task EnqueueWork(WorkMessageBase msg);

		/// <summary>
		///     Enqueues a work item to be dispatched on a worker with set queuing time for priority
		///     scheduling.
		/// </summary>
		/// <param name="msg"></param>
		Task EnqueueWork(WorkMessageBase msg, DateTime queueTime);

		/// <summary>
		///     Moves work item with given id to the start of the queue.
		/// </summary>
		/// <param name="id">Id of the job to prioritize.</param>
		Task PrioritizeWork(Guid id);

		/// <summary>
		///     Filters enqueued work items by the given filter.
		/// </summary>
		/// <param name="filter">Filter to use.</param>
		/// <returns></returns>
		Task<List<WorkItemDto>> FilterWork(WorkItemFilterDto filter);

		/// <summary>
		///     Cancels work job with given id.
		/// </summary>
		/// <param name="id">Id of the job to cancel.</param>
		Task CancelWork(Guid id);

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