using System;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Provides properties tracking timestamps of creation and modification
	/// </summary>
	public interface IChangeTrackable
	{
		/// <summary>
		///     Timestamp when this entity was created.
		/// </summary>
		DateTime Created { get; set; }

		/// <summary>
		///     Timestamp of last update of this entity.
		/// </summary>
		DateTime Updated { get; set; }
	}
}