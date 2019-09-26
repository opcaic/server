namespace OPCAIC.Application.Infrastructure
{
	public interface IIdentifiedRequest
	{
		/// <summary>
		///     Id of the entity this request is concerned in
		/// </summary>
		long Id { get; set; }
	}
}