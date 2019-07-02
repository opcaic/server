namespace OPCAIC.Infrastructure.Entities
{
	public abstract class SoftDeletableEntity : Entity, ISoftDeletable
	{
		/// <inheritdoc />
		public bool IsDeleted { get; set; }
	}
}