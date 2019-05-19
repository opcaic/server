using OPCAIC.Messaging.Messages;

namespace OPCAIC.Worker.Services
{
	/// <summary>
	///   Provides methods for executing services characterized by types of their request and result messages.
	/// </summary>
	/// <typeparam name="TRequest">Type of the request message.</typeparam>
	/// <typeparam name="TResult">Type of the result messages.</typeparam>
	public interface IJobExecutor<in TRequest, out TResult>
		where TRequest : WorkMessageBase where TResult : ReplyMessageBase
	{
		/// <summary>
		///   Executes the request.
		/// </summary>
		/// <param name="request">Input request object.</param>
		/// <returns></returns>
		TResult Execute(TRequest request);
	}
}
