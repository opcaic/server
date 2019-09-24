namespace OPCAIC.Application.Infrastructure
{
	/// <summary>
	///     Exposes id of the user who is making the request.
	/// </summary>
	public interface IUserRequest
	{
		/// <summary>
		///     Id of the user making the request.
		/// </summary>
		long? RequestingUserId { get; set; }
	}

	public abstract class UserRequest : IUserRequest
	{
		/// <inheritdoc />
		public long? RequestingUserId { get; set; }
	}
}