namespace OPCAIC.Infrastructure.Entities
{
	public interface ISoftDeletable
	{
		/// <summary>
		///     Flag whether the entity should be considered deleted (so we never actually delete any data).
		/// </summary>
		bool IsDeleted { get; set; }
	}
}