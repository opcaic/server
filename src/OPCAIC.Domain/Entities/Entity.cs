using System;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Base class for entities with timestamp tracking
	/// </summary>
	public abstract class Entity : EntityBase, IChangeTrackable
	{
		/// <inheritdoc />
		public DateTime Created { get; set; }

		/// <inheritdoc />
		public DateTime Updated { get; set; }
	}
}