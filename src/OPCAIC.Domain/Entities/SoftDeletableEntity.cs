namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Base class for entities which should be soft-deletable, meaning that it is never
	///     actually deleted, only marked as such and filtered out in all queries.
	/// </summary>
	public abstract class SoftDeletableEntity : Entity, ISoftDeletable
	{
		/// <inheritdoc />
		public bool IsDeleted { get; set; }
	}
}