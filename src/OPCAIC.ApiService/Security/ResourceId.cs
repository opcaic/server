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

		public long? Id { get; }

		private ResourceId()
		{
		}

		public ResourceId(long id)
		{
			Id = id;
		}
	}
}