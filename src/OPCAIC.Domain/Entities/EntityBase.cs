﻿namespace OPCAIC.Domain.Entities
{
	/// <summary>
	///     Base class for entities identifiable by a single unique Id.
	/// </summary>
	public abstract class EntityBase : IEntity
	{
		/// <summary>
		///     Primary key of this entity
		/// </summary>
		public long Id { get; set; }
	}
}