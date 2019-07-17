using OPCAIC.Infrastructure.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///   Base class for entities identifiable by a single unique Id.
	/// </summary>
	public abstract class Entity : IChangeTrackable
	{
		/// <summary>
		///   Primary key of this entity
		/// </summary>
		public long Id { get; set; }

		/// <inheritdoc />
		public DateTime Created { get; set; }

		/// <inheritdoc />
		public DateTime Updated { get; set; }
	}
}
