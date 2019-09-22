using System;

namespace OPCAIC.ApiService.Security
{
	/// <summary>
	///     Wrapper around long? type to provide strong typed reference to both null and non-null values.
	/// </summary>
	public class ResourceId
	{
		/// <summary>
		///    Singleton null instance.
		/// </summary>
		public static readonly ResourceId Null = new ResourceId();

		private readonly long? id;

		private ResourceId()
		{
		}

		public ResourceId(long id)
		{
			this.id = id;
		}

		/// <summary>
		///     Returns true if this id represent a concrete, non-null resource id.
		/// </summary>
		public bool HasValue => id.HasValue;

		/// <summary>
		///     If <see cref="HasValue"/>, returns the resource id.
		/// </summary>
		public long Value => id.Value;
	}
}