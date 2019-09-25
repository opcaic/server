using AutoMapper;

namespace OPCAIC.Application.Infrastructure.AutoMapper
{
	/// <summary>
	///     Marker interface that this type has custom mapping defined. This interface should be implemented explicitly in
	///     order to avoid polluting the method list.
	/// </summary>
	public interface ICustomMapping
	{
		/// <summary>
		///     Creates custom mappings for the given type in the provided <see cref="Profile" /> instance.
		///     Note that in order to correctly handle situations, when the implementing type has
		///     more derived types. Implementations should use the IncludeAllDerived() method on the
		///     mapping expression to make the mapping available in all derived classes.
		/// </summary>
		/// <param name="configuration"></param>
		void CreateMapping(Profile configuration);
	}
}